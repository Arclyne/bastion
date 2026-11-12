using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.GameModes;
using Bastion.Domain.Matchmaking;
using Bastion.Domain.Rooms;
using Bastion.Server.Application.Common;
using Bastion.Server.Application.Matchmaking;
using Bastion.Server.Application.Rooms;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Application.Tests.Fakes;
using Bastion.Server.Application.Tests.GameModes;
using Xunit;

namespace Bastion.Server.Application.Tests.Rooms;

public sealed class TestPrivateRoomService
{
    private const int HostAccountId = 7;
    private const string HostNickname = "marta_muros";
    private const string HostToken = "marta-token";
    private const int GuestAccountId = 8;
    private const string GuestNickname = "pedro_peon";
    private const string GuestToken = "pedro-token";
    private const int ThirdAccountId = 9;
    private const string ThirdNickname = "luis_quo";
    private const string ThirdToken = "luis-token";
    private const string UnknownToken = "unknown-token";
    private const string FirstCode = "K7MTQ3";
    private const string SecondCode = "P4XW92";
    private const string LowercaseFirstCode = "k7mtq3";
    private const string WrongCode = "AAAAAA";
    private const byte FiveMinuteClock = 5;
    private const byte TooManyWalls = 21;
    private const byte FewWalls = 4;
    private const int FirstSeatIndex = 0;
    private const int SecondSeatIndex = 1;

    private static readonly DateTime _now = new DateTime(2026, 11, 5, 18, 0, 0, DateTimeKind.Utc);
    private static readonly ConnectedPlayer _host = new ConnectedPlayer(HostAccountId, HostNickname);
    private static readonly ConnectedPlayer _guest = new ConnectedPlayer(GuestAccountId, GuestNickname);
    private static readonly ConnectedPlayer _third = new ConnectedPlayer(ThirdAccountId, ThirdNickname);

    private readonly FakeSessionVerifier _sessionVerifier = new FakeSessionVerifier();
    private readonly FakeMatchmakingNotifier _matchmakingNotifier = new FakeMatchmakingNotifier();
    private readonly MatchmakingQueue _matchmakingQueue = new MatchmakingQueue();
    private readonly FakeMatchRepository _matchRepository = new FakeMatchRepository();
    private readonly FakeRoomCodeGenerator _codeGenerator = new FakeRoomCodeGenerator();
    private readonly FakeRoomRepository _roomRepository = new FakeRoomRepository();
    private readonly FakeRoomNotifier _roomNotifier = new FakeRoomNotifier();
    private readonly RoomRegistry _roomRegistry;
    private readonly PrivateRoomService _privateRoomService;

    public TestPrivateRoomService()
    {
        _roomRegistry = new RoomRegistry(_codeGenerator);
        var playerActivity = new PlayerActivity(_matchmakingNotifier, _matchmakingQueue, _roomRegistry);
        var playerVerifier = new RoomPlayerVerifier(_sessionVerifier, _matchRepository, playerActivity);
        var roomKeeper = new RoomKeeper(_roomRegistry, _roomRepository, _roomNotifier);
        var settingsValidator = new RoomSettingsValidator(_matchRepository);
        _privateRoomService = new PrivateRoomService(playerVerifier, settingsValidator, roomKeeper);
    }

    [Fact]
    public void CreateRoom_InvalidSession_ThrowsInvalidSession()
    {
        RequestedRoomSettings settings = CreateClassicSettings();

        Action create = () => _privateRoomService.CreateRoom(UnknownToken, settings, _now);

        Assert.Throws<InvalidSessionException>(create);
    }

    [Fact]
    public void CreateRoom_PlayerWithoutNotifications_RejectsNotificationsUnavailable()
    {
        _sessionVerifier.Add(HostToken, _host);
        RequestedRoomSettings settings = CreateClassicSettings();
        Action create = () => _privateRoomService.CreateRoom(HostToken, settings, _now);

        Exception? exception = Record.Exception(create);

        var rejection = exception as RoomRejectedException;
        Assert.Equal(RoomRejection.NotificationsUnavailable, rejection?.Rejection);
    }

    [Fact]
    public void CreateRoom_PlayerSearchingMatch_RejectsSearchingMatch()
    {
        AddPlayer(HostToken, _host);
        var search = new MatchSearch(HostAccountId, HostNickname, _now);
        _matchmakingQueue.TryAdd(search);
        RequestedRoomSettings settings = CreateClassicSettings();
        Action create = () => _privateRoomService.CreateRoom(HostToken, settings, _now);

        Exception? exception = Record.Exception(create);

        var rejection = exception as RoomRejectedException;
        Assert.Equal(RoomRejection.SearchingMatch, rejection?.Rejection);
    }

    [Fact]
    public void CreateRoom_PlayerWithUnfinishedMatch_RejectsUnfinishedMatch()
    {
        AddPlayer(HostToken, _host);
        _matchRepository.AccountIdsWithUnfinishedParticipation.Add(HostAccountId);
        RequestedRoomSettings settings = CreateClassicSettings();
        Action create = () => _privateRoomService.CreateRoom(HostToken, settings, _now);

        Exception? exception = Record.Exception(create);

        var rejection = exception as RoomRejectedException;
        Assert.Equal(RoomRejection.UnfinishedMatch, rejection?.Rejection);
    }

    [Fact]
    public void CreateRoom_WallsOutOfRange_RejectsInvalidSettings()
    {
        AddPlayer(HostToken, _host);
        var settings = new RequestedRoomSettings(GameModeCode.Classic, TooManyWalls, FiveMinuteClock);
        Action create = () => _privateRoomService.CreateRoom(HostToken, settings, _now);

        Exception? exception = Record.Exception(create);

        var rejection = exception as RoomRejectedException;
        Assert.Equal(RoomRejection.InvalidSettings, rejection?.Rejection);
    }

    [Fact]
    public void CreateRoom_InactiveGameMode_RejectsInvalidSettings()
    {
        AddPlayer(HostToken, _host);
        var settings = new RequestedRoomSettings(GameModeCode.Quick, FewWalls, FiveMinuteClock);
        Action create = () => _privateRoomService.CreateRoom(HostToken, settings, _now);

        Exception? exception = Record.Exception(create);

        var rejection = exception as RoomRejectedException;
        Assert.Equal(RoomRejection.InvalidSettings, rejection?.Rejection);
    }

    [Fact]
    public void CreateRoom_ValidSettings_SavesOpenRoomWithCode()
    {
        AddHostWithCode();
        RequestedRoomSettings settings = CreateClassicSettings();
        (string Code, RoomStatus Status) expectedRoom = (FirstCode, RoomStatus.Open);

        _privateRoomService.CreateRoom(HostToken, settings, _now);

        Room savedRoom = _roomRepository.AddedRooms.Single();
        Assert.Equal(expectedRoom, (savedRoom.Code, savedRoom.Status));
    }

    [Fact]
    public void CreateRoom_ValidSettings_SeatsHostReadyInFirstSeat()
    {
        AddHostWithCode();
        RequestedRoomSettings settings = CreateClassicSettings();
        var expectedHost = new RoomMemberSnapshot
        {
            AccountId = HostAccountId,
            Nickname = HostNickname,
            SeatIndex = FirstSeatIndex,
            IsReady = true,
            IsHost = true,
        };

        RoomSnapshot room = _privateRoomService.CreateRoom(HostToken, settings, _now);

        RoomMemberSnapshot host = room.Members.Single();
        Assert.Equal(expectedHost, host);
    }

    [Fact]
    public void CreateRoom_HostAlreadyInRoom_RejectsInRoom()
    {
        OpenHostRoom();
        RequestedRoomSettings settings = CreateClassicSettings();
        Action create = () => _privateRoomService.CreateRoom(HostToken, settings, _now);

        Exception? exception = Record.Exception(create);

        var rejection = exception as RoomRejectedException;
        Assert.Equal(RoomRejection.InRoom, rejection?.Rejection);
    }

    [Fact]
    public void CreateRoom_CodeTakenInRepository_UsesNextCode()
    {
        AddHostWithCode();
        _codeGenerator.Codes.Enqueue(SecondCode);
        _roomRepository.TakenCodes.Add(FirstCode);
        RequestedRoomSettings settings = CreateClassicSettings();

        RoomSnapshot room = _privateRoomService.CreateRoom(HostToken, settings, _now);

        Assert.Equal(SecondCode, room.Identity.Code);
    }

    [Fact]
    public void JoinRoom_UnknownCode_RejectsRoomNotFound()
    {
        OpenHostRoom();
        AddPlayer(GuestToken, _guest);
        Action join = () => _privateRoomService.JoinRoom(GuestToken, WrongCode, _now);

        Exception? exception = Record.Exception(join);

        var rejection = exception as RoomRejectedException;
        Assert.Equal(RoomRejection.RoomNotFound, rejection?.Rejection);
    }

    [Fact]
    public void JoinRoom_LowercaseCode_SeatsPlayerNotReady()
    {
        OpenHostRoom();
        AddPlayer(GuestToken, _guest);
        var expectedGuest = new RoomMemberSnapshot
        {
            AccountId = GuestAccountId,
            Nickname = GuestNickname,
            SeatIndex = SecondSeatIndex,
            IsReady = false,
            IsHost = false,
        };

        RoomSnapshot room = _privateRoomService.JoinRoom(GuestToken, LowercaseFirstCode, _now);

        RoomMemberSnapshot guest = room.Members[SecondSeatIndex];
        Assert.Equal(expectedGuest, guest);
    }

    [Fact]
    public void JoinRoom_OpenRoom_NotifiesOthersFromJoiningPlayer()
    {
        OpenHostRoom();
        AddPlayer(GuestToken, _guest);

        _privateRoomService.JoinRoom(GuestToken, FirstCode, _now);

        NotifiedRoom update = _roomNotifier.UpdatedRooms.Single();
        Assert.Equal(GuestAccountId, update.OriginAccountId);
    }

    [Fact]
    public void JoinRoom_FullRoom_RejectsRoomFull()
    {
        OpenHostRoom();
        JoinGuest();
        AddPlayer(ThirdToken, _third);
        Action join = () => _privateRoomService.JoinRoom(ThirdToken, FirstCode, _now);

        Exception? exception = Record.Exception(join);

        var rejection = exception as RoomRejectedException;
        Assert.Equal(RoomRejection.RoomFull, rejection?.Rejection);
    }

    [Fact]
    public void JoinRoom_LimitOfWrongCodesReached_RejectsTooManyAttempts()
    {
        OpenHostRoom();
        AddPlayer(GuestToken, _guest);
        Action joinWithWrongCode = () => _privateRoomService.JoinRoom(GuestToken, WrongCode, _now);
        for (int attempt = 0; attempt < RoomPolicy.MaximumFailedCodeAttempts; attempt++)
        {
            Record.Exception(joinWithWrongCode);
        }
        Action join = () => _privateRoomService.JoinRoom(GuestToken, FirstCode, _now);

        Exception? exception = Record.Exception(join);

        var rejection = exception as RoomRejectedException;
        Assert.Equal(RoomRejection.TooManyAttempts, rejection?.Rejection);
    }

    [Fact]
    public void JoinRoom_RoomAlreadyInMatch_RejectsMatchStarted()
    {
        AddPlayer(GuestToken, _guest);
        _roomRepository.CodesInMatch.Add(FirstCode);
        Action join = () => _privateRoomService.JoinRoom(GuestToken, FirstCode, _now);

        Exception? exception = Record.Exception(join);

        var rejection = exception as RoomRejectedException;
        Assert.Equal(RoomRejection.MatchStarted, rejection?.Rejection);
    }

    [Fact]
    public void SetReady_PlayerNotInRoom_RejectsNotInRoom()
    {
        AddPlayer(GuestToken, _guest);
        Action setReady = () => _privateRoomService.SetReady(GuestToken, isReady: true, _now);

        Exception? exception = Record.Exception(setReady);

        var rejection = exception as RoomRejectedException;
        Assert.Equal(RoomRejection.NotInRoom, rejection?.Rejection);
    }

    [Fact]
    public void SetReady_Member_FillsTheRoomWithReadyPlayers()
    {
        OpenHostRoom();
        JoinGuest();
        bool[] expectedReadiness = { true, true };

        RoomSnapshot room = _privateRoomService.SetReady(GuestToken, isReady: true, _now);

        IEnumerable<bool> readiness = room.Members.Select(member => member.IsReady);
        Assert.Equal(expectedReadiness, readiness);
    }

    [Fact]
    public void ChangeSettings_Member_RejectsNotHost()
    {
        OpenHostRoom();
        JoinGuest();
        RequestedRoomSettings settings = CreateClassicSettings();
        Action change = () => _privateRoomService.ChangeSettings(GuestToken, settings, _now);

        Exception? exception = Record.Exception(change);

        var rejection = exception as RoomRejectedException;
        Assert.Equal(RoomRejection.NotHost, rejection?.Rejection);
    }

    [Fact]
    public void ChangeSettings_ReadyMember_ClearsReadyOfMember()
    {
        OpenHostRoom();
        JoinGuest();
        _privateRoomService.SetReady(GuestToken, isReady: true, _now);
        RequestedRoomSettings settings = CreateFewWallsSettings();

        RoomSnapshot room = _privateRoomService.ChangeSettings(HostToken, settings, _now);

        RoomMemberSnapshot guest = room.Members[SecondSeatIndex];
        Assert.False(guest.IsReady);
    }

    [Fact]
    public void ChangeSettings_Host_SavesSettingsOfRoom()
    {
        RoomSnapshot openedRoom = OpenHostRoom();
        int roomId = openedRoom.Identity.RoomId;
        RequestedRoomSettings settings = CreateFewWallsSettings();

        _privateRoomService.ChangeSettings(HostToken, settings, _now);

        RoomSettings savedSettings = _roomRepository.ChangedSettingsByRoomId[roomId];
        Assert.Equal(FewWalls, savedSettings.WallsPerPlayer);
    }

    [Fact]
    public void ChangeSettings_FourPlayerRoomToTwoSeatsWithThreeMembers_RejectsTooManyPlayers()
    {
        GameMode fourPlayerMode = GameModeSampleFactory.CreateFourPlayers();
        _matchRepository.ActiveGameMode = fourPlayerMode;
        AddHostWithCode();
        var fourPlayerSettings = new RequestedRoomSettings(GameModeCode.FourPlayers, FewWalls, FiveMinuteClock);
        _privateRoomService.CreateRoom(HostToken, fourPlayerSettings, _now);
        JoinGuest();
        AddPlayer(ThirdToken, _third);
        _privateRoomService.JoinRoom(ThirdToken, FirstCode, _now);
        GameMode classicMode = GameModeSampleFactory.CreateClassic();
        _matchRepository.ActiveGameMode = classicMode;
        RequestedRoomSettings classicSettings = CreateClassicSettings();
        Action change = () => _privateRoomService.ChangeSettings(HostToken, classicSettings, _now);

        Exception? exception = Record.Exception(change);

        var rejection = exception as RoomRejectedException;
        Assert.Equal(RoomRejection.TooManyPlayers, rejection?.Rejection);
    }

    [Fact]
    public void LeaveRoom_Member_NotifiesRemainingMembers()
    {
        OpenHostRoom();
        JoinGuest();

        _privateRoomService.LeaveRoom(GuestToken, _now);

        NotifiedRoom lastUpdate = _roomNotifier.UpdatedRooms.Last();
        Assert.Single(lastUpdate.Room.Members);
    }

    [Fact]
    public void LeaveRoom_Host_ClosesRoomInRepository()
    {
        RoomSnapshot openedRoom = OpenHostRoom();
        int[] expectedClosedRoomIds = { openedRoom.Identity.RoomId };

        _privateRoomService.LeaveRoom(HostToken, _now);

        Assert.Equal(expectedClosedRoomIds, _roomRepository.ClosedRoomIds);
    }

    [Fact]
    public void LeaveRoom_Host_NotifiesMembersRoomClosed()
    {
        OpenHostRoom();
        JoinGuest();

        _privateRoomService.LeaveRoom(HostToken, _now);

        NotifiedRoom closure = _roomNotifier.ClosedRooms.Single();
        Assert.Equal(HostAccountId, closure.OriginAccountId);
    }

    [Fact]
    public void LeaveRoom_Host_FreesMembers()
    {
        OpenHostRoom();
        JoinGuest();

        _privateRoomService.LeaveRoom(HostToken, _now);

        bool isGuestInRoom = _roomRegistry.IsInRoom(GuestAccountId);
        Assert.False(isGuestInRoom);
    }

    [Fact]
    public void FindOwnRoom_PlayerNotInRoom_ReturnsNull()
    {
        AddPlayer(GuestToken, _guest);

        RoomSnapshot? room = _privateRoomService.FindOwnRoom(GuestToken, _now);

        Assert.Null(room);
    }

    private RoomSnapshot OpenHostRoom()
    {
        AddHostWithCode();
        RequestedRoomSettings settings = CreateClassicSettings();
        return _privateRoomService.CreateRoom(HostToken, settings, _now);
    }

    private void AddHostWithCode()
    {
        AddPlayer(HostToken, _host);
        _codeGenerator.Codes.Enqueue(FirstCode);
    }

    private void AddPlayer(string sessionToken, ConnectedPlayer player)
    {
        _sessionVerifier.Add(sessionToken, player);
        _matchmakingNotifier.SubscribedAccountIds.Add(player.AccountId);
    }

    private static RequestedRoomSettings CreateClassicSettings()
    {
        GameMode classicMode = GameModeSampleFactory.CreateClassic();
        return new RequestedRoomSettings(GameModeCode.Classic, classicMode.WallsPerPlayer, FiveMinuteClock);
    }

    private void JoinGuest()
    {
        AddPlayer(GuestToken, _guest);
        _privateRoomService.JoinRoom(GuestToken, FirstCode, _now);
    }

    private static RequestedRoomSettings CreateFewWallsSettings()
    {
        return new RequestedRoomSettings(GameModeCode.Classic, FewWalls, FiveMinuteClock);
    }
}
