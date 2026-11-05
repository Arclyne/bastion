using System;
using System.ServiceModel;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Notifications;
using Bastion.Contracts;
using Bastion.Contracts.Chat;

namespace Bastion.Client.Networking.Chat;

public sealed class ChatClient : IChatClient, IDisposable
{
    private readonly ServiceChannel<IChatService> _channel =
        new ServiceChannel<IChatService>(ServiceEndpoints.ChatPath);
    private readonly INotificationClient _notificationClient;

    public ChatClient(INotificationClient notificationClient)
    {
        ArgumentNullException.ThrowIfNull(notificationClient);

        _notificationClient = notificationClient;
    }

    public event EventHandler<MatchChatMessageReceivedEventArgs>? MatchChatMessageReceived
    {
        add => _notificationClient.MatchChatMessageReceived += value;
        remove => _notificationClient.MatchChatMessageReceived -= value;
    }

    public void SendLobbyChatMessage(string sessionToken, string text)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(text);

        Call(service => service.SendLobbyChatMessage(sessionToken, text));
    }

    public void SendMatchChatMessage(string sessionToken, int matchId, string text)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(text);

        Call(service => service.SendMatchChatMessage(sessionToken, matchId, text));
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

        if (ex is FaultException<ChatChannelForbiddenFault>)
        {
            return new ChatChannelForbiddenException(ex);
        }

        return ServiceFailureTranslator.ToClientException(ex);
    }

    private void Call(Action<IChatService> operation)
    {
        try
        {
            _channel.Call(operation);
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
}
