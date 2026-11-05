using System;
using System.Data.Common;
using Bastion.Contracts.Chat;
using Bastion.Server.Application.Chat;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Host.Sessions;
using CoreWCF;
using log4net;
using Microsoft.Extensions.DependencyInjection;

namespace Bastion.Server.Host.Chat;

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

    public ChatMessageReceipt SendChatMessage(string? sessionToken, string? text)
    {
        string token = RequireSessionToken(sessionToken);
        if (text is null)
        {
            throw ChatTranslator.ToFault(new ChatMessageRejectedException(ChatRejection.InvalidMessage));
        }

        return Run(
            nameof(SendChatMessage),
            chat => ChatTranslator.ToReceipt(chat.SendMessage(token, text, GetUtcNow())));
    }

    public LobbyChatLog OpenLobbyChat(string? sessionToken)
    {
        string token = RequireSessionToken(sessionToken);
        return Run(nameof(OpenLobbyChat), chat => ChatTranslator.ToChatLog(chat.OpenLobbyChat(token, GetUtcNow())));
    }

    private static string RequireSessionToken(string? sessionToken)
    {
        return sessionToken ?? throw SessionTranslator.ToFault(new InvalidSessionException());
    }

    private TResult Run<TResult>(string operationName, Func<ILobbyChatService, TResult> operation)
    {
        try
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            return operation(scope.ServiceProvider.GetRequiredService<ILobbyChatService>());
        }
        catch (InvalidSessionException ex)
        {
            throw SessionTranslator.ToFault(ex);
        }
        catch (ChatMessageRejectedException ex)
        {
            throw ChatTranslator.ToFault(ex);
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
