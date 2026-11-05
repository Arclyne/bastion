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
        string checkedText = RequireText(text);
        return Run<ILobbyChatService, ChatMessageReceipt>(
            nameof(SendChatMessage),
            chat => ChatTranslator.ToReceipt(chat.SendMessage(token, checkedText, GetUtcNow())));
    }

    /// <inheritdoc/>
    public LobbyChatLog OpenLobbyChat(string? sessionToken)
    {
        string token = RequireSessionToken(sessionToken);
        return Run<ILobbyChatService, LobbyChatLog>(
            nameof(OpenLobbyChat),
            chat => ChatTranslator.ToChatLog(chat.OpenLobbyChat(token, GetUtcNow())));
    }

    /// <inheritdoc/>
    public ChatMessageReceipt SendMatchChatMessage(string? sessionToken, int matchId, string? text)
    {
        string token = RequireSessionToken(sessionToken);
        var submission = new MatchChatSubmission(matchId, RequireText(text));
        return Run<IMatchChatService, ChatMessageReceipt>(
            nameof(SendMatchChatMessage),
            chat => ChatTranslator.ToReceipt(chat.SendMessage(token, submission, GetUtcNow())));
    }

    private static string RequireSessionToken(string? sessionToken)
    {
        return sessionToken ?? throw SessionTranslator.ToFault(new InvalidSessionException());
    }

    private static string RequireText(string? text)
    {
        return text ?? throw ChatTranslator.ToFault(new ChatMessageRejectedException(ChatRejection.InvalidMessage));
    }

    private TResult Run<TChatService, TResult>(string operationName, Func<TChatService, TResult> operation)
        where TChatService : notnull
    {
        try
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            return operation(scope.ServiceProvider.GetRequiredService<TChatService>());
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
        catch (MatchStoreException ex)
        {
            _logger.Error($"The match store could not be used. Operation={operationName}", ex);
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
