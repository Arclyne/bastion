using System;
using System.ServiceModel;
using log4net;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Sessions;
using Bastion.Contracts;
using Bastion.Contracts.Chat;
using Bastion.Contracts.Sessions;

namespace Bastion.Client.Networking.Chat;

public sealed class ChatClient : IChatClient, IDisposable
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(ChatClient));

    private readonly ServiceChannel<IChatService> _channel =
        new ServiceChannel<IChatService>(ServiceEndpoints.ChatPath);

    public void Send(string sessionToken, string text)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(text);

        try
        {
            _channel.Call(service => service.SendChatMessage(sessionToken, text));
        }
        catch (Exception ex) when (ex is CommunicationException || ex is TimeoutException)
        {
            throw ToClientException(ex);
        }
    }

    public void Dispose()
    {
        _channel.Dispose();
    }

    private static Exception ToClientException(Exception ex)
    {
        switch (ex)
        {
            case EndpointNotFoundException:
                _logger.Warn("The server could not be reached.", ex);
                return new ServerUnreachableException(ex);
            case FaultException<InvalidSessionFault>:
                _logger.Warn("The server turned down the session.", ex);
                return new InvalidSessionException(ex);
            case FaultException<InvalidChatMessageFault>:
                _logger.Warn("The server turned down the chat message.", ex);
                return new InvalidChatMessageException(ex);
            case FaultException:
                _logger.Warn("The server could not deliver the chat message.", ex);
                return new ServerErrorException(ex);
            default:
                _logger.Warn("The connection with the server was lost.", ex);
                return new ConnectionLostException(ex);
        }
    }
}
