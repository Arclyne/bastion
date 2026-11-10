using System;
using System.Linq;
using Bastion.Domain.GameModes;
using Bastion.Domain.Matchmaking;
using Bastion.Domain.Rooms;
using Bastion.Server.Application.Common;
using Bastion.Server.Application.Matchmaking;
using Bastion.Server.Application.Rooms;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Application.Tests.Fakes;
using Xunit;

namespace Bastion.Server.Application.Tests.Rooms;

public class TestPrivateRoomService
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

    private readonly FakeSessionVerifier _sessionVerifier = new FakeSessionVerifier();
    private readonly FakeMatchmakingNotifier _matchmakingNotifier = new FakeMatchmakingNotifier();
    private readonly MatchmakingQueue _matchmakingQueue = new MatchmakingQueue();
    private readonly FakeMatchStore _matchStore = new FakeMatchStore();
    private readonly FakeRoomCodeGenerator _codeGenerator = new FakeRoomCodeGenerator();
    private readonly FakeRoomStore _roomStore = new FakeRoomStore();
    private readonly FakeRoomNotifier _roomNotifier = new FakeRoomNotifier();
    private readonly RoomRegistry _roomRegistry;
    private readonly PrivateRoomService _privateRoomService;

    public TestPrivateRoomService()
    {
        _roomRegistry = new RoomRegistry(_codeGenerator);
        var playerActivity = new PlayerActivity(_matchmakingNotifier, _matchmakingQueue, _roomRegistry);
        var playerVerifier = new RoomPlayerVerifier(_sessionVerifier, _matchStore, playerActivity);
        var roomKeeper = new RoomKeeper(_roomRegistry, _roomStore, _roomNotifier);
        var settingsValidator = new RoomSettingsValidator(_matchStore);
        _privateRoomService = new PrivateRoomService(playerVerifier, settingsValidator, roomKeeper);
        AddPlayer(HostToken, HostAccountId, HostNickname);
        AddPlayer(GuestToken, GuestAccountId, GuestNickname);
        AddPlayer(ThirdToken, ThirdAccountId, ThirdNickname);
        _codeGenerator.Codes.Enqueue(FirstCode);
    }

    [Fact]
    public void CreateRoom_InvalidSession_ThrowsInvalidSession()
    {
        Assert.Throws<InvalidSessionException>(
            () => _privateRoomService.CreateRoom(UnknownToken, CreateClassicSettings(), _now));
    }

    [Fact]
    public void CreateRoom_PlayerWithoutNotifications_RejectsNotificationsUnavailable()
    {
        _matchmakingNotifier.SubscribedAccountIds.Remove(HostAccountId);

        RoomRejection rejection = CatchRejection(() => CreateHostRoom());

        Assert.Equal(RoomRejection.NotificationsUnavailable, rejection);
    }

    [Fact]
    public void CreateRoom_PlayerSearchingMatch_RejectsSearchingMatch()
    {
        _matchmakingQueue.TryAdd(new MatchSearch(HostAccountId, HostNickname, _now));

        RoomRejection rejection = CatchRejection(() => CreateHostRoom());

        Assert.Equal(RoomRejection.SearchingMatch, rejection);
    }

    [Fact]
    public void CreateRoom_PlayerWithUnfinishedMatch_RejectsUnfinishedMatch()
    {
        _matchStore.AccountIdsWithUnfinishedParticipation.Add(HostAccountId);

        RoomRejection rejection = CatchRejection(() => CreateHostRoom());

        Assert.Equal(RoomRejection.UnfinishedMatch, rejection);
    }

    [Fact]
    public void CreateRoom_WallsOutOfRange_RejectsInvalidSettings()
    {
        var requestedSettings = new RequestedRoomSettings(GameModeCode.Classic, TooManyWalls, FiveMinuteClock);

        RoomRejection rejection =
            CatchRejection(() => _privateRoomService.CreateRoom(HostToken, requestedSettings, _now));

        Assert.Equal(RoomRejection.InvalidSettings, rejection);
    }

    [Fact]
    public void CreateRoom_InactiveGameMode_RejectsInvalidSettings()
    {
        var requestedSettings = new RequestedRoomSettings(GameModeCode.Quick, FewWalls, FiveMinuteClock);

        RoomRejection rejection =
            CatchRejection(() => _privateRoomService.CreateRoom(HostToken, requestedSettings, _now));

        Assert.Equal(RoomRejection.InvalidSettings, rejection);
    }

    [Fact]
    public void CreateRoom_ValidSettings_SavesOpenRoomWithCode()
    {
        CreateHostRoom();

        Room savedRoom = _roomStore.AddedRooms.Single();
        Assert.Equal((FirstCode, RoomStatus.Open), (savedRoom.Code, savedRoom.Status));
    }

    [Fact]
    public void CreateRoom_ValidSettings_SeatsHostReadyInFirstSeat()
    {
        var expectedHost = new RoomMemberSnapshot(HostAccountId, HostNickname, FirstSeatIndex, true, true);

        RoomSnapshot room = CreateHostRoom();

        Assert.Equal(expectedHost, room.Members.Single());
    }

    [Fact]
    public void CreateRoom_HostAlreadyInRoom_RejectsInRoom()
    {
        CreateHostRoom();

        RoomRejection rejection = CatchRejection(() => CreateHostRoom());

        Assert.Equal(RoomRejection.InRoom, rejection);
    }

    [Fact]
    public void CreateRoom_CodeTakenInStore_UsesNextCode()
    {
        _roomStore.TakenCodes.Add(FirstCode);
        _codeGenerator.Codes.Enqueue(SecondCode);

        RoomSnapshot room = CreateHostRoom();

        Assert.Equal(SecondCode, room.Identity.Code);
    }

    [Fact]
    public void JoinRoom_UnknownCode_RejectsRoomNotFound()
    {
        CreateHostRoom();

        RoomRejection rejection = CatchRejection(() => _privateRoomService.JoinRoom(GuestToken, WrongCode, _now));

        Assert.Equal(RoomRejection.RoomNotFound, rejection);
    }

    [Fact]
    public void JoinRoom_LowercaseCode_SeatsPlayerNotReady()
    {
        CreateHostRoom();
        var expectedGuest = new RoomMemberSnapshot(GuestAccountId, GuestNickname, SecondSeatIndex, false, false);

        RoomSnapshot room = _privateRoomService.JoinRoom(GuestToken, LowercaseFirstCode, _now);

        Assert.Equal(expectedGuest, room.Members[SecondSeatIndex]);
    }

    [Fact]
    public void JoinRoom_OpenRoom_NotifiesOthersFromJoiningPlayer()
    {
        CreateHostRoom();

        _privateRoomService.JoinRoom(GuestToken, FirstCode, _now);

        Assert.Equal(GuestAccountId, _roomNotifier.UpdatedRooms.Single().OriginAccountId);
    }

    [Fact]
    public void JoinRoom_FullRoom_RejectsRoomFull()
    {
        CreateHostRoom();
        _privateRoomService.JoinRoom(GuestToken, FirstCode, _now);

        RoomRejection rejection = CatchRejection(() => _privateRoomService.JoinRoom(ThirdToken, FirstCode, _now));

        Assert.Equal(RoomRejection.RoomFull, rejection);
    }

    [Fact]
    public void JoinRoom_LimitOfWrongCodesReached_RejectsTooManyAttempts()
    {
        CreateHostRoom();
        for (int attempt = 0; attempt < RoomPolicy.MaximumFailedCodeAttempts; attempt++)
        {
            CatchRejection(() => _privateRoomService.JoinRoom(GuestToken, WrongCode, _now));
        }

        RoomRejection rejection = CatchRejection(() => _privateRoomService.JoinRoom(GuestToken, FirstCode, _now));

        Assert.Equal(RoomRejection.TooManyAttempts, rejection);
    }

    [Fact]
    public void SetReady_PlayerNotInRoom_RejectsNotInRoom()
    {
        RoomRejection rejection = CatchRejection(() => _privateRoomService.SetReady(GuestToken, true, _now));

        Assert.Equal(RoomRejection.NotInRoom, rejection);
    }

    [Fact]
    public void SetReady_Member_FillsTheRoomWithReadyPlayers()
    {
        CreateHostRoom();
        _privateRoomService.JoinRoom(GuestToken, FirstCode, _now);

        RoomSnapshot room = _privateRoomService.SetReady(GuestToken, true, _now);

        Assert.True(room.Members.All(member => member.IsReady));
    }

    [Fact]
    public void ChangeSettings_Member_RejectsNotHost()
    {
        CreateHostRoom();
        _privateRoomService.JoinRoom(GuestToken, FirstCode, _now);

        RoomRejection rejection =
            CatchRejection(() => _privateRoomService.ChangeSettings(GuestToken, CreateClassicSettings(), _now));

        Assert.Equal(RoomRejection.NotHost, rejection);
    }

    [Fact]
    public void ChangeSettings_ReadyMember_ClearsReadyOfMember()
    {
        CreateHostRoom();
        _privateRoomService.JoinRoom(GuestToken, FirstCode, _now);
        _privateRoomService.SetReady(GuestToken, true, _now);
        var requestedSettings = new RequestedRoomSettings(GameModeCode.Classic, FewWalls, FiveMinuteClock);

        RoomSnapshot room = _privateRoomService.ChangeSettings(HostToken, requestedSettings, _now);

        Assert.False(room.Members[SecondSeatIndex].IsReady);
    }

    [Fact]
    public void ChangeSettings_Host_SavesSettingsOfRoom()
    {
        RoomSnapshot openedRoom = CreateHostRoom();
        var requestedSettings = new RequestedRoomSettings(GameModeCode.Classic, FewWalls, FiveMinuteClock);

        _privateRoomService.ChangeSettings(HostToken, requestedSettings, _now);

        Assert.Equal(FewWalls, _roomStore.ChangedSettingsByRoomId[openedRoom.Identity.RoomId].WallsPerPlayer);
    }

    [Fact]
    public void ChangeSettings_FourPlayerRoomToTwoSeatsWithThreeMembers_RejectsTooManyPlayers()
    {
        _matchStore.ActiveGameMode = GameModeSamples.CreateFourPlayers();
        var fourPlayerSettings = new RequestedRoomSettings(GameModeCode.FourPlayers, FewWalls, FiveMinuteClock);
        _privateRoomService.CreateRoom(HostToken, fourPlayerSettings, _now);
        _privateRoomService.JoinRoom(GuestToken, FirstCode, _now);
        _privateRoomService.JoinRoom(ThirdToken, FirstCode, _now);
        _matchStore.ActiveGameMode = GameModeSamples.CreateClassic();

        RoomRejection rejection =
            CatchRejection(() => _privateRoomService.ChangeSettings(HostToken, CreateClassicSettings(), _now));

        Assert.Equal(RoomRejection.TooManyPlayers, rejection);
    }

    [Fact]
    public void LeaveRoom_Member_NotifiesRemainingMembers()
    {
        CreateHostRoom();
        _privateRoomService.JoinRoom(GuestToken, FirstCode, _now);

        _privateRoomService.LeaveRoom(GuestToken, _now);

        Assert.Single(_roomNotifier.UpdatedRooms.Last().Room.Members);
    }

    [Fact]
    public void LeaveRoom_Host_ClosesRoomInStore()
    {
        RoomSnapshot openedRoom = CreateHostRoom();

        _privateRoomService.LeaveRoom(HostToken, _now);

        Assert.Equal(openedRoom.Identity.RoomId, _roomStore.ClosedRoomIds.Single());
    }

    [Fact]
    public void LeaveRoom_Host_NotifiesMembersRoomClosed()
    {
        CreateHostRoom();
        _privateRoomService.JoinRoom(GuestToken, FirstCode, _now);

        _privateRoomService.LeaveRoom(HostToken, _now);

        Assert.Equal(HostAccountId, _roomNotifier.ClosedRooms.Single().OriginAccountId);
    }

    [Fact]
    public void LeaveRoom_Host_FreesMembers()
    {
        CreateHostRoom();
        _privateRoomService.JoinRoom(GuestToken, FirstCode, _now);

        _privateRoomService.LeaveRoom(HostToken, _now);

        Assert.False(_roomRegistry.IsInRoom(GuestAccountId));
    }

    [Fact]
    public void FindOwnRoom_PlayerNotInRoom_ReturnsNull()
    {
        RoomSnapshot? room = _privateRoomService.FindOwnRoom(GuestToken, _now);

        Assert.Null(room);
    }

    private static RequestedRoomSettings CreateClassicSettings()
    {
        return new RequestedRoomSettings(GameModeCode.Classic, GameModeSamples.ClassicWallsPerPlayer, FiveMinuteClock);
    }

    private static RoomRejection CatchRejection(Action request)
    {
        return Assert.Throws<RoomRejectedException>(request).Rejection;
    }

    private RoomSnapshot CreateHostRoom()
    {
        return _privateRoomService.CreateRoom(HostToken, CreateClassicSettings(), _now);
    }

    private void AddPlayer(string sessionToken, int accountId, string nickname)
    {
        _sessionVerifier.Add(sessionToken, new ConnectedPlayer(accountId, nickname));
        _matchmakingNotifier.SubscribedAccountIds.Add(accountId);
    }
}
