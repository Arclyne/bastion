using System;
using System.Data.Common;
using CoreWCF;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using Bastion.Contracts.Chat;
using Bastion.Server.Application.Chat;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Host.Sessions;

namespace Bastion.Server.Host.Chat;

[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
public sealed class ChatService : IChatService
{
    private const string ServerFailureReason = "The server could not deliver the chat message.";

    private static readonly ILog _logger = LogManager.GetLogger(typeof(ChatService));

    private readonly IServiceScopeFactory _scopeFactory;

    public ChatService(IServiceScopeFactory scopeFactory)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);

        _scopeFactory = scopeFactory;
    }

    public void SendChatMessage(string sessionToken, string text)
    {
        try
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            ILobbyChatService chat = scope.ServiceProvider.GetRequiredService<ILobbyChatService>();
            chat.Send(sessionToken ?? string.Empty, text ?? string.Empty, DateTime.UtcNow);
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
