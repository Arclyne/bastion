using System;

namespace Bastion.Client.Networking.Notifications;

public interface INotificationClient
{
    event EventHandler<LobbyChatMessageReceivedEventArgs>? LobbyChatMessageReceived;

    event EventHandler<MatchChatMessageReceivedEventArgs>? MatchChatMessageReceived;

    event EventHandler<MatchFoundEventArgs>? MatchFound;

    event EventHandler<PresenceRequestedEventArgs>? PresenceRequested;

    event EventHandler<SearchEndedEventArgs>? SearchEnded;

    event EventHandler? Disconnected;

    void Connect(string sessionToken);

    void Disconnect();
}
