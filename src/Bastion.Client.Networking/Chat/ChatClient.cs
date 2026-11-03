using System;
using System.ServiceModel;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Sessions;
using Bastion.Contracts;
using Bastion.Contracts.Chat;
using Bastion.Contracts.Sessions;

namespace Bastion.Client.Networking.Chat;

public sealed class ChatClient : IChatClient, IDisposable
{
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
        catch (CommunicationException ex)
        {
            throw ToClientException(ex);
        }
        catch (TimeoutException ex)
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
                return new ServerUnreachableException(ex);
            case FaultException<InvalidSessionFault>:
                return new InvalidSessionException(ex);
            case FaultException<InvalidChatMessageFault>:
                return new InvalidChatMessageException(ex);
            case FaultException:
                return new ServerErrorException(ex);
            case CommunicationException:
            case TimeoutException:
                return new ConnectionLostException(ex);
            default:
                throw new ArgumentOutOfRangeException(nameof(ex), ex.GetType(), "Unsupported exception type.");
        }
    }
}
