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
    private const string ServerFailureReason = "The server could not deliver the chat message.";

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
    public void SendLobbyChatMessage(string? sessionToken, string? text)
    {
        RunChatRequest(sessionToken, text, CallLobbyChatService);
    }

    /// <inheritdoc/>
    public void SendMatchChatMessage(string? sessionToken, int matchId, string? text)
    {
        RunChatRequest(sessionToken, text, CreateMatchChatServiceCall(matchId));
    }

    private Action<IServiceProvider, string, string> CreateMatchChatServiceCall(int matchId)
    {
        return (provider, token, text) => CallMatchChatService(provider, token, new MatchChatSubmission(matchId, text));
    }

    private void CallLobbyChatService(IServiceProvider provider, string sessionToken, string text)
    {
        provider.GetRequiredService<ILobbyChatService>().SendMessage(sessionToken, text, GetUtcNow());
    }

    private void CallMatchChatService(IServiceProvider provider, string sessionToken, MatchChatSubmission submission)
    {
        provider.GetRequiredService<IMatchChatService>().SendMessage(sessionToken, submission, GetUtcNow());
    }

    private void RunChatRequest(string? sessionToken, string? text, Action<IServiceProvider, string, string> send)
    {
        if (sessionToken is null)
        {
            throw SessionTranslator.ToFault(new InvalidSessionException());
        }

        if (text is null)
        {
            throw ChatTranslator.ToFault(new InvalidChatMessageException());
        }

        try
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            send(scope.ServiceProvider, sessionToken, text);
        }
        catch (InvalidSessionException ex)
        {
            throw SessionTranslator.ToFault(ex);
        }
        catch (ChatChannelForbiddenException ex)
        {
            throw ChatTranslator.ToFault(ex);
        }
        catch (InvalidChatMessageException ex)
        {
            throw ChatTranslator.ToFault(ex);
        }
        catch (MatchStoreException ex)
        {
            _logger.Error("The match store could not be used while sending a chat message.", ex);
            throw new FaultException(ServerFailureReason);
        }
        catch (DbException ex)
        {
            _logger.Error("The database could not be reached while sending a chat message.", ex);
            throw new FaultException(ServerFailureReason);
        }
    }

    private DateTime GetUtcNow()
    {
        return _timeProvider.GetUtcNow().UtcDateTime;
    }
}
