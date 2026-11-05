using System;
using System.Data.Common;
using Bastion.Contracts.Chat;
using Bastion.Contracts.PrivateChat;
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

    public ConversationEntry[] ListConversations(string? sessionToken)
    {
        string token = RequireSessionToken(sessionToken);
        return Run(
            nameof(ListConversations),
            chat => PrivateChatTranslator.ToConversationEntries(chat.ListConversations(token, GetUtcNow())));
    }

    public PrivateConversationLog OpenConversation(string? sessionToken, string? friendCode)
    {
        string token = RequireSessionToken(sessionToken);
        string code = RequireFriendCode(friendCode);
        return Run(
            nameof(OpenConversation),
            chat => PrivateChatTranslator.ToConversationLog(chat.OpenConversation(token, code, GetUtcNow())));
    }

    public ChatMessageReceipt SendPrivateMessage(string? sessionToken, string? friendCode, string? text)
    {
        string token = RequireSessionToken(sessionToken);
        string code = RequireFriendCode(friendCode);
        if (text is null)
        {
            throw ChatTranslator.ToFault(new ChatMessageRejectedException(ChatRejection.InvalidMessage));
        }

        var draft = new PrivateMessageDraft(code, text);
        return Run(
            nameof(SendPrivateMessage),
            chat => PrivateChatTranslator.ToReceipt(chat.SendPrivateMessage(token, draft, GetUtcNow())));
    }

    public void MarkConversationRead(string? sessionToken, string? friendCode)
    {
        string token = RequireSessionToken(sessionToken);
        string code = RequireFriendCode(friendCode);
        Run(nameof(MarkConversationRead), chat =>
        {
            chat.MarkConversationRead(token, code, GetUtcNow());
            return true;
        });
    }

    private static string RequireSessionToken(string? sessionToken)
    {
        return sessionToken ?? throw SessionTranslator.ToFault(new InvalidSessionException());
    }

    private static string RequireFriendCode(string? friendCode)
    {
        return friendCode
            ?? throw ChatTranslator.ToFault(new ChatMessageRejectedException(ChatRejection.PlayerNotFound));
    }

    private TResult Run<TResult>(string operationName, Func<IPrivateChatService, TResult> operation)
    {
        try
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            return operation(scope.ServiceProvider.GetRequiredService<IPrivateChatService>());
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
            _logger.Error($"The private message could not be saved. Operation={operationName}", ex);
            throw new FaultException(ServerFailureReason);
        }
        catch (DbException ex)
        {
            _logger.Error($"The database could not be reached. Operation={operationName}", ex);
            throw new FaultException(ServerFailureReason);
        }
    }

    private DateTime GetUtcNow()
    {
        return _timeProvider.GetUtcNow().UtcDateTime;
    }
}
