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

    public void SendChatMessage(string? sessionToken, string? text)
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
            ILobbyChatService chat = scope.ServiceProvider.GetRequiredService<ILobbyChatService>();
            chat.Send(sessionToken, text, _timeProvider.GetUtcNow().UtcDateTime);
        }
        catch (InvalidSessionException ex)
        {
            throw SessionTranslator.ToFault(ex);
        }
        catch (InvalidChatMessageException ex)
        {
            throw ChatTranslator.ToFault(ex);
        }
        catch (DbException ex)
        {
            _logger.Error("The database could not be reached while sending a chat message.", ex);
            throw new FaultException(ServerFailureReason);
        }
    }
}
