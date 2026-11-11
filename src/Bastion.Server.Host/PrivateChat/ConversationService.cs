using System;
using System.Collections.Generic;
using System.Data.Common;
using Bastion.Contracts.Chat;
using Bastion.Contracts.PrivateChat;
using Bastion.Domain.Chat;
using Bastion.Server.Application.Chat;
using Bastion.Server.Application.PrivateChat;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Host.Chat;
using Bastion.Server.Host.Sessions;
using CoreWCF;
using log4net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bastion.Server.Host.PrivateChat;

/// <inheritdoc/>
[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
public sealed class ConversationService : IConversationService
{
    private const string ServerFailureReason = "The server could not complete the conversation operation.";

    private static readonly ILog _logger = LogManager.GetLogger(typeof(ConversationService));

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;

    public ConversationService(IServiceScopeFactory scopeFactory, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc/>
    public ConversationEntry[] ListConversations(string? sessionToken)
    {
        string token = SessionTranslator.RequireSessionToken(sessionToken);
        Func<ConversationEntry[]> listConversations = () => ListConversationsInRequestScope(token);
        return TranslateConversationFailures(nameof(ListConversations), listConversations);
    }

    /// <inheritdoc/>
    public PrivateConversationLog OpenConversation(string? sessionToken, string? friendCode)
    {
        string token = SessionTranslator.RequireSessionToken(sessionToken);
        string partnerCode = RequireFriendCode(friendCode);
        Func<PrivateConversationLog> openConversation = () => OpenConversationInRequestScope(token, partnerCode);
        return TranslateConversationFailures(nameof(OpenConversation), openConversation);
    }

    /// <inheritdoc/>
    public ChatMessageReceipt SendPrivateMessage(string? sessionToken, string? friendCode, string? text)
    {
        string token = SessionTranslator.RequireSessionToken(sessionToken);
        string recipientCode = RequireFriendCode(friendCode);
        string messageText = ChatTranslator.RequireText(text);
        var draft = new PrivateMessageDraft(recipientCode, messageText);
        Func<ChatMessageReceipt> sendMessage = () => SendPrivateMessageInRequestScope(token, draft);
        return TranslateConversationFailures(nameof(SendPrivateMessage), sendMessage);
    }

    /// <inheritdoc/>
    public void MarkConversationRead(string? sessionToken, string? friendCode)
    {
        string token = SessionTranslator.RequireSessionToken(sessionToken);
        string partnerCode = RequireFriendCode(friendCode);
        Action markConversationRead = () => MarkConversationReadInRequestScope(token, partnerCode);
        TranslateConversationFailures(nameof(MarkConversationRead), markConversationRead);
    }

    private static string RequireFriendCode(string? friendCode)
    {
        return friendCode
            ?? throw ChatTranslator.ToFault(new ChatMessageRejectedException(ChatRejection.PlayerNotFound));
    }

    private ConversationEntry[] ListConversationsInRequestScope(string token)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IPrivateChatService privateChat = scope.ServiceProvider.GetRequiredService<IPrivateChatService>();
        IReadOnlyList<ConversationSummary> summaries = privateChat.ListConversations(token, GetUtcNow());
        return PrivateChatTranslator.ToConversationEntries(summaries);
    }

    private PrivateConversationLog OpenConversationInRequestScope(string token, string partnerCode)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IPrivateChatService privateChat = scope.ServiceProvider.GetRequiredService<IPrivateChatService>();
        PrivateConversation conversation = privateChat.OpenConversation(token, partnerCode, GetUtcNow());
        return PrivateChatTranslator.ToConversationLog(conversation);
    }

    private ChatMessageReceipt SendPrivateMessageInRequestScope(string token, PrivateMessageDraft draft)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IPrivateChatService privateChat = scope.ServiceProvider.GetRequiredService<IPrivateChatService>();
        Message message = privateChat.SendPrivateMessage(token, draft, GetUtcNow());
        return PrivateChatTranslator.ToReceipt(message);
    }

    private void MarkConversationReadInRequestScope(string token, string partnerCode)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IPrivateChatService privateChat = scope.ServiceProvider.GetRequiredService<IPrivateChatService>();
        privateChat.MarkConversationRead(token, partnerCode, GetUtcNow());
    }

    private TResult TranslateConversationFailures<TResult>(string operationName, Func<TResult> operation)
    {
        try
        {
            return operation();
        }
        catch (InvalidSessionException ex)
        {
            throw SessionTranslator.ToFault(ex);
        }
        catch (ChatMessageRejectedException ex)
        {
            throw ChatTranslator.ToFault(ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.Error($"The conversation could not be saved. Operation={operationName}", ex);
            throw new FaultException(ServerFailureReason);
        }
        catch (DbException ex)
        {
            _logger.Error($"The database could not be reached. Operation={operationName}", ex);
            throw new FaultException(ServerFailureReason);
        }
    }

    private void TranslateConversationFailures(string operationName, Action operation)
    {
        Func<bool> operationAsFunction = () => CompleteOperation(operation);
        TranslateConversationFailures(operationName, operationAsFunction);
    }

    private static bool CompleteOperation(Action operation)
    {
        operation();
        return true;
    }

    private DateTime GetUtcNow()
    {
        return _timeProvider.GetUtcNow().UtcDateTime;
    }
}
