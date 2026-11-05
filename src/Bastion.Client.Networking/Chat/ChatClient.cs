using System;
using System.ServiceModel;
using Bastion.Client.Networking.Channels;
using Bastion.Contracts;
using Bastion.Contracts.Chat;

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
        if (ex is FaultException<InvalidChatMessageFault>)
        {
            return new InvalidChatMessageException(ex);
        }

        return ServiceFailureTranslator.ToClientException(ex);
    }
}
