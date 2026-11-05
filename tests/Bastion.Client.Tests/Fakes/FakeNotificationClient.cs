using System;
using Bastion.Client.Networking.Notifications;

namespace Bastion.Client.Tests.Fakes;

public sealed class FakeNotificationClient : INotificationClient
{
    public event EventHandler<ChatMessageReceivedEventArgs>? ChatMessageReceived;

    public event EventHandler<MatchChatMessageReceivedEventArgs>? MatchChatMessageReceived;

    public event EventHandler<MatchAbandonedEventArgs>? MatchAbandoned;

    public event EventHandler<MatchFoundEventArgs>? MatchFound;

    public event EventHandler<PresenceRequestedEventArgs>? PresenceRequested;

    public event EventHandler<SearchEndedEventArgs>? SearchEnded;

    public event EventHandler<FriendPresenceChangedEventArgs>? FriendPresenceChanged;

    public event EventHandler<FriendRequestReceivedEventArgs>? FriendRequestReceived;

    public event EventHandler<FriendshipStartedEventArgs>? FriendshipStarted;

    public event EventHandler<PrivateMessageReceivedEventArgs>? PrivateMessageReceived;

    public event EventHandler<RoomUpdatedEventArgs>? RoomUpdated;

    public event EventHandler<RoomClosedEventArgs>? RoomClosed;

    public event EventHandler? Disconnected;

    public void Connect(string sessionToken)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
    }

    public void Disconnect()
    {
    }

    public bool HasAnySubscribers()
    {
        var handlers = new Delegate?[]
        {
            ChatMessageReceived,
            MatchChatMessageReceived,
            MatchAbandoned,
            MatchFound,
            PresenceRequested,
            SearchEnded,
            FriendPresenceChanged,
            FriendRequestReceived,
            FriendshipStarted,
            PrivateMessageReceived,
            RoomUpdated,
            RoomClosed,
            Disconnected,
        };
        return Array.Exists(handlers, handler => handler is not null);
    }
}
