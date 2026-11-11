using System;
using System.Data.Common;
using Bastion.Contracts.Chat;
using Bastion.Domain.Chat;
using Bastion.Server.Application.Chat;
using Bastion.Server.Application.Matches;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Host.Sessions;
using CoreWCF;
using log4net;
using Microsoft.Extensions.DependencyInjection;

namespace Bastion.Server.Host.Chat;

/// <inheritdoc/>
[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
public sealed class ChatService : IChatService
{
    private const string ServerFailureReason = "The server could not complete the chat operation.";

    private static readonly ILog _logger = LogManager.GetLogger(typeof(ChatService));

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;

    public ChatService(IServiceScopeFactory scopeFactory, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc/>
    public ChatMessageReceipt SendChatMessage(string? sessionToken, string? text)
    {
        string token = RequireSessionToken(sessionToken);
        string messageText = RequireText(text);
        Func<ChatMessageReceipt> sendMessage = () => SendChatMessageInRequestScope(token, messageText);
        return TranslateChatFailures(nameof(SendChatMessage), sendMessage);
    }

    /// <inheritdoc/>
    public LobbyChatLog OpenLobbyChat(string? sessionToken)
    {
        string token = RequireSessionToken(sessionToken);
        Func<LobbyChatLog> openChat = () => OpenLobbyChatInRequestScope(token);
        return TranslateChatFailures(nameof(OpenLobbyChat), openChat);
    }

    /// <inheritdoc/>
    public ChatMessageReceipt SendMatchChatMessage(string? sessionToken, int matchId, string? text)
    {
        string token = RequireSessionToken(sessionToken);
        string messageText = RequireText(text);
        var submission = new MatchChatSubmission(matchId, messageText);
        Func<ChatMessageReceipt> sendMessage = () => SendMatchChatMessageInRequestScope(token, submission);
        return TranslateChatFailures(nameof(SendMatchChatMessage), sendMessage);
    }

    private static string RequireSessionToken(string? sessionToken)
    {
        return sessionToken ?? throw SessionTranslator.ToFault(new InvalidSessionException());
    }

    private static string RequireText(string? text)
    {
        return text ?? throw ChatTranslator.ToFault(new ChatMessageRejectedException(ChatRejection.InvalidMessage));
    }

    private ChatMessageReceipt SendChatMessageInRequestScope(string token, string messageText)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ILobbyChatService lobbyChat = scope.ServiceProvider.GetRequiredService<ILobbyChatService>();
        PostedChatLine postedLine = lobbyChat.SendMessage(token, messageText, GetUtcNow());
        return ChatTranslator.ToReceipt(postedLine);
    }

    private LobbyChatLog OpenLobbyChatInRequestScope(string token)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ILobbyChatService lobbyChat = scope.ServiceProvider.GetRequiredService<ILobbyChatService>();
        LobbyChatHistory history = lobbyChat.OpenLobbyChat(token, GetUtcNow());
        return ChatTranslator.ToChatLog(history);
    }

    private ChatMessageReceipt SendMatchChatMessageInRequestScope(string token, MatchChatSubmission submission)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IMatchChatService matchChat = scope.ServiceProvider.GetRequiredService<IMatchChatService>();
        PostedChatLine postedLine = matchChat.SendMessage(token, submission, GetUtcNow());
        return ChatTranslator.ToReceipt(postedLine);
    }

    private TResult TranslateChatFailures<TResult>(string operationName, Func<TResult> operation)
    {
        try
        {
            return operation();
        }
        catch (InvalidSessionException ex)
        {
            throw SessionTranslator.ToFault(ex);
        }
        catch (ChatChannelForbiddenException ex)
        {
            throw ChatTranslator.ToFault(ex);
        }
        catch (ChatMessageRejectedException ex)
        {
            throw ChatTranslator.ToFault(ex);
        }
        catch (MatchRepositoryException ex)
        {
            _logger.Error($"The match repository could not be used. Operation={operationName}", ex);
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
