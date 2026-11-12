using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Client.Networking.Friends;
using Bastion.Client.Networking.Matchmaking;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Networking.Rooms;
using Bastion.Contracts.Friends;
using Bastion.Contracts.Notifications;
using Bastion.Contracts.Rooms;
using Xunit;

namespace Bastion.Client.Tests.Notifications;

public sealed class TestNotificationCallback
{
    private const string FirstNickname = "marta_muros";
    private const string SecondNickname = "leo_peon";
    private const string MessageText = "hello";
    private const int ResponseSeconds = 60;
    private const string FriendCode = "MA3R7T";
    private const string SenderFriendCode = "QU7K2P";
    private const int FoundMatchId = 12;
    private const int ChatMatchId = 21;
    private const int AbandonedMatchId = 30;
    private const byte ClassicBoardSize = 9;
    private const byte ClassicWallsPerPlayer = 10;
    private const byte ClockMinutes = 5;
    private const short FirstEloRating = 1210;
    private const short SecondEloRating = 1000;
    private const byte FirstTurnOrder = 1;
    private const byte SecondTurnOrder = 2;
    private const string RoomCode = "K7MTQ3";
    private const string HostNickname = "marta_muros";
    private const string GuestNickname = "pedro_peon";
    private const byte RoomWallsPerPlayer = 6;
    private const byte RoomClockMinutes = 3;
    private const int TwoSeats = 2;
    private const int FirstSeatIndex = 0;
    private const int SecondSeatIndex = 1;

    private static readonly DateTime _sentAt = new DateTime(2026, 11, 4, 18, 15, 0, DateTimeKind.Utc);
    private static readonly string[] _participantNicknames = { FirstNickname, SecondNickname };

    private readonly NotificationCallback _notificationCallback = new NotificationCallback();
    private ChatMessageReceivedEventArgs? _chatMessage;
    private MatchChatMessageReceivedEventArgs? _matchChatMessage;
    private MatchAbandonedEventArgs? _matchAbandoned;
    private MatchFoundEventArgs? _matchFound;
    private PresenceRequestedEventArgs? _presenceRequested;
    private SearchEndedEventArgs? _searchEnded;
    private FriendPresenceChangedEventArgs? _friendPresence;
    private FriendRequestReceivedEventArgs? _friendRequest;
    private FriendshipStartedEventArgs? _friendshipStarted;
    private PrivateMessageReceivedEventArgs? _privateMessage;
    private RoomUpdatedEventArgs? _roomUpdated;
    private RoomClosedEventArgs? _roomClosed;

    [Fact]
    public void ReceiveNotification_FriendRequest_RaisesRequesterNickname()
    {
        _notificationCallback.FriendRequestReceived += HandleFriendRequestReceived;
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
        _notificationCallback.FriendshipStarted += HandleFriendshipStarted;
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
        _notificationCallback.FriendPresenceChanged += HandleFriendPresenceChanged;
        FriendPresenceNotification notification = CreateFriendPresenceNotification();

        _notificationCallback.ReceiveNotification(notification);

        Assert.Equal(FriendCode, _friendPresence?.Friend.Friend.FriendCode);
    }

    [Fact]
    public void ReceiveNotification_FriendPresence_RaisesConnectedPresence()
    {
        _notificationCallback.FriendPresenceChanged += HandleFriendPresenceChanged;
        FriendPresenceNotification notification = CreateFriendPresenceNotification();

        _notificationCallback.ReceiveNotification(notification);

        Assert.Equal(FriendPresence.Connected, _friendPresence?.Friend.Presence);
    }

    [Fact]
    public void ReceiveNotification_PrivateMessage_RaisesText()
    {
        _notificationCallback.PrivateMessageReceived += HandlePrivateMessageReceived;
        var notification = new PrivateMessageNotification
        {
            SenderNickname = FirstNickname,
            SenderFriendCode = SenderFriendCode,
            Text = MessageText,
            OccurredAt = _sentAt,
        };

        _notificationCallback.ReceiveNotification(notification);

        Assert.Equal(MessageText, _privateMessage?.Message.Text);
    }

    [Fact]
    public void ReceiveNotification_ChatMessage_RaisesSenderNickname()
    {
        _notificationCallback.ChatMessageReceived += HandleChatMessageReceived;
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
        _notificationCallback.MatchChatMessageReceived += HandleMatchChatMessageReceived;
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
    public void ReceiveNotification_MatchAbandoned_RaisesAbandoningNickname()
    {
        _notificationCallback.MatchAbandoned += HandleMatchAbandoned;
        var notification = new MatchAbandonedNotification
        {
            MatchId = AbandonedMatchId,
            AbandoningNickname = FirstNickname,
            OccurredAt = _sentAt,
        };

        _notificationCallback.ReceiveNotification(notification);

        Assert.Equal(FirstNickname, _matchAbandoned?.AbandoningNickname);
    }

    [Fact]
    public void ReceiveNotification_MatchFound_RaisesMatchFormat()
    {
        _notificationCallback.MatchFound += HandleMatchFound;
        MatchFoundNotification notification = CreateMatchFoundNotification();
        var expectedFormat = new FoundMatchFormat(ClassicBoardSize, ClassicWallsPerPlayer, ClockMinutes);

        _notificationCallback.ReceiveNotification(notification);

        Assert.Equal(expectedFormat, _matchFound?.Format);
    }

    [Fact]
    public void ReceiveNotification_MatchFound_RaisesBothNicknames()
    {
        _notificationCallback.MatchFound += HandleMatchFound;
        MatchFoundNotification notification = CreateMatchFoundNotification();

        _notificationCallback.ReceiveNotification(notification);

        IEnumerable<string>? nicknames = _matchFound?.Participants.Select(participant => participant.Nickname);
        Assert.Equal(_participantNicknames, nicknames);
    }

    [Fact]
    public void ReceiveNotification_MatchFound_RaisesMatchId()
    {
        _notificationCallback.MatchFound += HandleMatchFound;
        MatchFoundNotification notification = CreateMatchFoundNotification();

        _notificationCallback.ReceiveNotification(notification);

        Assert.Equal(FoundMatchId, _matchFound?.MatchId);
    }

    [Fact]
    public void ReceiveNotification_PresenceRequested_RaisesServerResponseWindow()
    {
        _notificationCallback.PresenceRequested += HandlePresenceRequested;
        DateTime deadline = _sentAt.AddSeconds(ResponseSeconds);
        var notification = new PresenceRequestedNotification
        {
            OccurredAt = _sentAt,
            Deadline = deadline,
        };
        var expectedWindow = TimeSpan.FromSeconds(ResponseSeconds);

        _notificationCallback.ReceiveNotification(notification);

        Assert.Equal(expectedWindow, _presenceRequested?.ResponseWindow);
    }

    [Fact]
    public void ReceiveNotification_SearchEnded_RaisesReason()
    {
        _notificationCallback.SearchEnded += HandleSearchEnded;
        var notification = new SearchEndedNotification
        {
            Reason = SearchEndedReason.PresenceExpired,
            OccurredAt = _sentAt,
        };

        _notificationCallback.ReceiveNotification(notification);

        Assert.Equal(SearchEndReason.PresenceExpired, _searchEnded?.Reason);
    }

    [Fact]
    public void ReceiveNotification_RoomUpdated_RaisesMembersInSeatOrder()
    {
        _notificationCallback.RoomUpdated += HandleRoomUpdated;
        RoomUpdatedNotification notification = CreateRoomUpdatedNotification();
        string[] expectedNicknames = { HostNickname, GuestNickname };

        _notificationCallback.ReceiveNotification(notification);

        IEnumerable<string>? nicknames = _roomUpdated?.Room.Members.Select(member => member.Nickname);
        Assert.Equal(expectedNicknames, nicknames);
    }

    [Fact]
    public void ReceiveNotification_RoomUpdated_RaisesRoomOptions()
    {
        _notificationCallback.RoomUpdated += HandleRoomUpdated;
        RoomUpdatedNotification notification = CreateRoomUpdatedNotification();
        var expectedOptions = new PrivateRoomOptions(PrivateRoomGameMode.Quick, RoomWallsPerPlayer, RoomClockMinutes);

        _notificationCallback.ReceiveNotification(notification);

        Assert.Equal(expectedOptions, _roomUpdated?.Room.Options);
    }

    [Fact]
    public void ReceiveNotification_RoomClosed_RaisesClosedCode()
    {
        _notificationCallback.RoomClosed += HandleRoomClosed;
        var notification = new RoomClosedNotification
        {
            OccurredAt = _sentAt,
            Code = RoomCode,
            Reason = RoomClosedReason.HostLeft,
        };

        _notificationCallback.ReceiveNotification(notification);

        Assert.Equal(RoomCode, _roomClosed?.Code);
    }

    [Fact]
    public void ReceiveNotification_UnknownNotification_DoesNotThrow()
    {
        var notification = new UnknownNotification
        {
            OccurredAt = _sentAt,
        };
        Action receive = () => _notificationCallback.ReceiveNotification(notification);

        Exception? exception = Record.Exception(receive);

        Assert.Null(exception);
    }

    private void HandleFriendRequestReceived(object? sender, FriendRequestReceivedEventArgs e)
    {
        _friendRequest = e;
    }

    private void HandleFriendshipStarted(object? sender, FriendshipStartedEventArgs e)
    {
        _friendshipStarted = e;
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

    private void HandlePrivateMessageReceived(object? sender, PrivateMessageReceivedEventArgs e)
    {
        _privateMessage = e;
    }

    private void HandleChatMessageReceived(object? sender, ChatMessageReceivedEventArgs e)
    {
        _chatMessage = e;
    }

    private void HandleMatchChatMessageReceived(object? sender, MatchChatMessageReceivedEventArgs e)
    {
        _matchChatMessage = e;
    }

    private void HandleMatchAbandoned(object? sender, MatchAbandonedEventArgs e)
    {
        _matchAbandoned = e;
    }

    private void HandleMatchFound(object? sender, MatchFoundEventArgs e)
    {
        _matchFound = e;
    }

    private static MatchFoundNotification CreateMatchFoundNotification()
    {
        var firstParticipant = new MatchFoundParticipant
        {
            Nickname = FirstNickname,
            EloRating = FirstEloRating,
            TurnOrder = FirstTurnOrder,
        };
        var secondParticipant = new MatchFoundParticipant
        {
            Nickname = SecondNickname,
            EloRating = SecondEloRating,
            TurnOrder = SecondTurnOrder,
        };
        MatchFoundParticipant[] participants = { firstParticipant, secondParticipant };
        return new MatchFoundNotification
        {
            MatchId = FoundMatchId,
            BoardSize = ClassicBoardSize,
            WallsPerPlayer = ClassicWallsPerPlayer,
            ClockMinutes = ClockMinutes,
            Participants = participants,
            OccurredAt = _sentAt,
        };
    }

    private void HandlePresenceRequested(object? sender, PresenceRequestedEventArgs e)
    {
        _presenceRequested = e;
    }

    private void HandleSearchEnded(object? sender, SearchEndedEventArgs e)
    {
        _searchEnded = e;
    }

    private void HandleRoomUpdated(object? sender, RoomUpdatedEventArgs e)
    {
        _roomUpdated = e;
    }

    private static RoomUpdatedNotification CreateRoomUpdatedNotification()
    {
        var guest = new RoomMemberState
        {
            Nickname = GuestNickname,
            SeatIndex = SecondSeatIndex,
        };
        var host = new RoomMemberState
        {
            Nickname = HostNickname,
            SeatIndex = FirstSeatIndex,
            IsReady = true,
            IsHost = true,
        };
        var options = new RoomOptions
        {
            GameMode = RoomGameMode.Quick,
            WallsPerPlayer = RoomWallsPerPlayer,
            ClockMinutes = RoomClockMinutes,
        };
        RoomMemberState[] members = { guest, host };
        var room = new RoomState
        {
            Code = RoomCode,
            SeatCount = TwoSeats,
            Options = options,
            Members = members,
        };
        return new RoomUpdatedNotification
        {
            OccurredAt = _sentAt,
            Room = room,
        };
    }

    private void HandleRoomClosed(object? sender, RoomClosedEventArgs e)
    {
        _roomClosed = e;
    }

    private sealed class UnknownNotification : Notification
    {
    }
}
