using System;
using Bastion.Client.Networking.Notifications;

namespace Bastion.Client.Tests.Fakes;

public sealed class FakeNotificationClient : INotificationClient
{
    public event EventHandler<ChatMessageReceivedEventArgs>? ChatMessageReceived;

    public event EventHandler<MatchChatMessageReceivedEventArgs>? MatchChatMessageReceived;

    public event EventHandler<ParticipationEndedEventArgs>? ParticipationEnded;

    public event EventHandler<MatchEndedEventArgs>? MatchEnded;

    public event EventHandler<PawnMovedEventArgs>? PawnMoved;

    public event EventHandler<WallPlacedEventArgs>? WallPlaced;

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

    public event EventHandler<OpponentConnectionEventArgs>? OpponentDisconnected;

    public event EventHandler<OpponentConnectionEventArgs>? OpponentReconnected;

    public event EventHandler? MatchTakenOver;

    public event EventHandler<DrawEventArgs>? DrawOffered;

    public event EventHandler<DrawEventArgs>? DrawRejected;

    public event EventHandler<DrawEventArgs>? DrawOfferExpired;

    public Exception? ConnectFailure { get; set; }

    public int ConnectCount { get; private set; }

    public void Connect(string sessionToken)
    {
        if (ConnectFailure is not null)
        {
            throw ConnectFailure;
        }

        ConnectCount++;
    }

    public void RaiseDisconnected()
    {
        Disconnected?.Invoke(this, EventArgs.Empty);
    }

    public bool HasAnySubscribers()
    {
        var handlers = new Delegate?[]
        {
            ChatMessageReceived,
            MatchChatMessageReceived,
            ParticipationEnded,
            MatchEnded,
            PawnMoved,
            WallPlaced,
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
            OpponentDisconnected,
            OpponentReconnected,
            MatchTakenOver,
            DrawOffered,
            DrawRejected,
            DrawOfferExpired,
        };
        return Array.Exists(handlers, handler => handler is not null);
    }
}
