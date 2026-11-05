using System;
using System.Linq;
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
    private const int FoundMatchId = 12;
    private const int ChatMatchId = 21;
    private const byte ClassicBoardSize = 9;
    private const byte ClassicWallsPerPlayer = 10;
    private const byte ClockMinutes = 5;
    private const short FirstEloRating = 1210;
    private const short SecondEloRating = 1000;
    private const byte FirstTurnOrder = 1;
    private const byte SecondTurnOrder = 2;

    private static readonly DateTime _sentAt = new DateTime(2026, 11, 4, 18, 15, 0, DateTimeKind.Utc);
    private static readonly string[] _participantNicknames = new string[]
    {
        FirstNickname,
        SecondNickname,
    };

    private static readonly MatchFoundParticipant[] _participants = new MatchFoundParticipant[]
    {
        new MatchFoundParticipant
        {
            Nickname = FirstNickname,
            EloRating = FirstEloRating,
            TurnOrder = FirstTurnOrder,
        },
        new MatchFoundParticipant
        {
            Nickname = SecondNickname,
            EloRating = SecondEloRating,
            TurnOrder = SecondTurnOrder,
        },
    };

    private readonly NotificationCallback _notificationCallback = new NotificationCallback();
    private ChatMessageReceivedEventArgs? _chatMessage;
    private MatchChatMessageReceivedEventArgs? _matchChatMessage;
    private MatchFoundEventArgs? _matchFound;
    private PresenceRequestedEventArgs? _presenceRequested;
    private SearchEndedEventArgs? _searchEnded;
    private FriendPresenceChangedEventArgs? _friendPresence;
    private FriendRequestReceivedEventArgs? _friendRequest;
    private FriendshipStartedEventArgs? _friendshipStarted;
    private PrivateMessageReceivedEventArgs? _privateMessage;

    public TestNotificationCallback()
    {
        _notificationCallback.ChatMessageReceived += HandleChatMessageReceived;
        _notificationCallback.MatchChatMessageReceived += HandleMatchChatMessageReceived;
        _notificationCallback.MatchFound += HandleMatchFound;
        _notificationCallback.PresenceRequested += HandlePresenceRequested;
        _notificationCallback.SearchEnded += HandleSearchEnded;
        _notificationCallback.FriendPresenceChanged += HandleFriendPresenceChanged;
        _notificationCallback.FriendRequestReceived += HandleFriendRequestReceived;
        _notificationCallback.FriendshipStarted += HandleFriendshipStarted;
        _notificationCallback.PrivateMessageReceived += HandlePrivateMessageReceived;
    }

    [Fact]
    public void ReceiveNotification_FriendRequest_RaisesRequesterNickname()
    {
        var notification = new FriendRequestNotification
        {
            RequesterNickname = FirstNickname,
            RequesterFriendCode = FriendCode,
            OccurredAt = _sentAt,
        };

        _notificationCallback.ReceiveNotification(notification);

        Assert.Equal(FirstNickname, _friendRequest?.Request.Player.Nickname);
    }

    [Fact]
    public void ReceiveNotification_FriendshipStarted_RaisesFriendCode()
    {
        var notification = new FriendshipStartedNotification
        {
            FriendNickname = FirstNickname,
            FriendCode = FriendCode,
            OccurredAt = _sentAt,
        };

        _notificationCallback.ReceiveNotification(notification);

        Assert.Equal(FriendCode, _friendshipStarted?.Friend.FriendCode);
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
    public void ReceiveNotification_PrivateMessage_RaisesText()
    {
        var notification = new PrivateMessageNotification
        {
            SenderNickname = FirstNickname,
            SenderFriendCode = "QU7K2P",
            Text = MessageText,
            OccurredAt = _sentAt,
        };

        _notificationCallback.ReceiveNotification(notification);

        Assert.Equal(MessageText, _privateMessage?.Message.Text);
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

        Assert.Equal(FirstNickname, _chatMessage?.Message.Author.Nickname);
    }

    [Fact]
    public void ReceiveNotification_MatchChatMessage_RaisesMatchId()
    {
        var notification = new MatchChatMessageNotification
        {
            MatchId = ChatMatchId,
            SenderNickname = FirstNickname,
            Text = MessageText,
            OccurredAt = _sentAt,
        };

        _notificationCallback.ReceiveNotification(notification);

        Assert.Equal(ChatMatchId, _matchChatMessage?.MatchId);
    }

    [Fact]
    public void ReceiveNotification_MatchFound_RaisesMatchFormat()
    {
        MatchFoundNotification notification = CreateMatchFoundNotification();

        _notificationCallback.ReceiveNotification(notification);

        Assert.Equal(new FoundMatchFormat(ClassicBoardSize, ClassicWallsPerPlayer, ClockMinutes), _matchFound?.Format);
    }

    [Fact]
    public void ReceiveNotification_MatchFound_RaisesBothNicknames()
    {
        MatchFoundNotification notification = CreateMatchFoundNotification();

        _notificationCallback.ReceiveNotification(notification);

        Assert.Equal(_participantNicknames, _matchFound?.Participants.Select(participant => participant.Nickname));
    }

    [Fact]
    public void ReceiveNotification_MatchFound_RaisesMatchId()
    {
        MatchFoundNotification notification = CreateMatchFoundNotification();

        _notificationCallback.ReceiveNotification(notification);

        Assert.Equal(FoundMatchId, _matchFound?.MatchId);
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

    private static MatchFoundNotification CreateMatchFoundNotification()
    {
        return new MatchFoundNotification
        {
            MatchId = FoundMatchId,
            BoardSize = ClassicBoardSize,
            WallsPerPlayer = ClassicWallsPerPlayer,
            ClockMinutes = ClockMinutes,
            Participants = _participants,
            OccurredAt = _sentAt,
        };
    }

    private void HandleMatchChatMessageReceived(object? sender, MatchChatMessageReceivedEventArgs e)
    {
        _matchChatMessage = e;
    }

    private void HandlePrivateMessageReceived(object? sender, PrivateMessageReceivedEventArgs e)
    {
        _privateMessage = e;
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

    private void HandleFriendRequestReceived(object? sender, FriendRequestReceivedEventArgs e)
    {
        _friendRequest = e;
    }

    private void HandleFriendshipStarted(object? sender, FriendshipStartedEventArgs e)
    {
        _friendshipStarted = e;
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
