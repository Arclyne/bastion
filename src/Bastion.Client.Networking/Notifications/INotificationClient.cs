using System;

namespace Bastion.Client.Networking.Notifications;

public interface INotificationClient
{
    event EventHandler<ChatMessageReceivedEventArgs>? ChatMessageReceived;

    event EventHandler<MatchChatMessageReceivedEventArgs>? MatchChatMessageReceived;

    event EventHandler<MatchAbandonedEventArgs>? MatchAbandoned;

    event EventHandler<MatchFoundEventArgs>? MatchFound;

    event EventHandler<PresenceRequestedEventArgs>? PresenceRequested;

    event EventHandler<SearchEndedEventArgs>? SearchEnded;

    event EventHandler<FriendPresenceChangedEventArgs>? FriendPresenceChanged;

    event EventHandler<FriendRequestReceivedEventArgs>? FriendRequestReceived;

    event EventHandler<FriendshipStartedEventArgs>? FriendshipStarted;

    event EventHandler<PrivateMessageReceivedEventArgs>? PrivateMessageReceived;

    event EventHandler? Disconnected;

    void Connect(string sessionToken);

    void Disconnect();
}
