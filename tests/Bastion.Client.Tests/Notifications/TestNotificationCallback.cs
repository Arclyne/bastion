using System;
using Bastion.Client.Networking.Friends;
using Bastion.Client.Networking.Matchmaking;
using Bastion.Client.Networking.Notifications;
using Bastion.Contracts.Friends;
using Bastion.Contracts.Notifications;
using Xunit;

namespace Bastion.Client.Tests.Notifications;

public class TestNotificationCallback
{
    private const string FirstNickname = "marta_muros";
    private const string SecondNickname = "leo_peon";
    private const string MessageText = "hello";
    private const int ResponseSeconds = 60;
    private const string FriendCode = "MA3R7T";

    private static readonly DateTime _sentAt = new DateTime(2026, 11, 4, 18, 15, 0, DateTimeKind.Utc);
    private static readonly string[] _participantNicknames = new string[]
    {
        FirstNickname,
        SecondNickname,
    };

    private readonly NotificationCallback _notificationCallback = new NotificationCallback();
    private ChatMessageReceivedEventArgs? _chatMessage;
    private MatchFoundEventArgs? _matchFound;
    private PresenceRequestedEventArgs? _presenceRequested;
    private SearchEndedEventArgs? _searchEnded;
    private FriendPresenceChangedEventArgs? _friendPresence;

    public TestNotificationCallback()
    {
        _notificationCallback.ChatMessageReceived += HandleChatMessageReceived;
        _notificationCallback.MatchFound += HandleMatchFound;
        _notificationCallback.PresenceRequested += HandlePresenceRequested;
        _notificationCallback.SearchEnded += HandleSearchEnded;
        _notificationCallback.FriendPresenceChanged += HandleFriendPresenceChanged;
    }

    [Fact]
    public void ReceiveNotification_FriendPresence_RaisesFriendCode()
    {
        _notificationCallback.ReceiveNotification(CreateFriendPresenceNotification());

        Assert.Equal(FriendCode, _friendPresence?.Friend.Friend.FriendCode);
    }

    [Fact]
    public void ReceiveNotification_FriendPresence_RaisesConnectedPresence()
    {
        _notificationCallback.ReceiveNotification(CreateFriendPresenceNotification());

        Assert.Equal(FriendPresence.Connected, _friendPresence?.Friend.Presence);
    }

    [Fact]
    public void ReceiveNotification_ChatMessage_RaisesSenderNickname()
    {
        var notification = new ChatMessageNotification
        {
            SenderNickname = FirstNickname,
            Text = MessageText,
            OccurredAt = _sentAt,
        };

        _notificationCallback.ReceiveNotification(notification);

        Assert.Equal(FirstNickname, _chatMessage?.SenderNickname);
    }

    [Fact]
    public void ReceiveNotification_MatchFound_RaisesBothNicknames()
    {
        var notification = new MatchFoundNotification
        {
            ParticipantNicknames = _participantNicknames,
            OccurredAt = _sentAt,
        };

        _notificationCallback.ReceiveNotification(notification);

        Assert.Equal(_participantNicknames, _matchFound?.ParticipantNicknames);
    }

    [Fact]
    public void ReceiveNotification_PresenceRequested_RaisesServerResponseWindow()
    {
        var notification = new PresenceRequestedNotification
        {
            OccurredAt = _sentAt,
            Deadline = _sentAt.AddSeconds(ResponseSeconds),
        };

        _notificationCallback.ReceiveNotification(notification);

        Assert.Equal(TimeSpan.FromSeconds(ResponseSeconds), _presenceRequested?.ResponseWindow);
    }

    [Fact]
    public void ReceiveNotification_SearchEnded_RaisesReason()
    {
        var notification = new SearchEndedNotification
        {
            Reason = SearchEndedReason.PresenceExpired,
            OccurredAt = _sentAt,
        };

        _notificationCallback.ReceiveNotification(notification);

        Assert.Equal(SearchEndReason.PresenceExpired, _searchEnded?.Reason);
    }

    [Fact]
    public void ReceiveNotification_UnknownNotification_DoesNotThrow()
    {
        var notification = new UnknownNotification
        {
            OccurredAt = _sentAt,
        };

        Exception? exception = Record.Exception(() => _notificationCallback.ReceiveNotification(notification));

        Assert.Null(exception);
    }

    private void HandleChatMessageReceived(object? sender, ChatMessageReceivedEventArgs e)
    {
        _chatMessage = e;
    }

    private void HandleMatchFound(object? sender, MatchFoundEventArgs e)
    {
        _matchFound = e;
    }

    private void HandlePresenceRequested(object? sender, PresenceRequestedEventArgs e)
    {
        _presenceRequested = e;
    }

    private void HandleSearchEnded(object? sender, SearchEndedEventArgs e)
    {
        _searchEnded = e;
    }

    private void HandleFriendPresenceChanged(object? sender, FriendPresenceChangedEventArgs e)
    {
        _friendPresence = e;
    }

    private static FriendPresenceNotification CreateFriendPresenceNotification()
    {
        return new FriendPresenceNotification
        {
            FriendNickname = FirstNickname,
            FriendCode = FriendCode,
            Presence = PresenceState.Connected,
            OccurredAt = _sentAt,
        };
    }

    private sealed class UnknownNotification : Notification
    {
    }
}
