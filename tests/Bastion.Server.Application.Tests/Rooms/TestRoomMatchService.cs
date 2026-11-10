using System;
using System.Linq;
using Bastion.Domain.GameModes;
using Bastion.Domain.Matches;
using Bastion.Server.Application.Common;
using Bastion.Server.Application.Matchmaking;
using Bastion.Server.Application.Rooms;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Application.Tests.Fakes;
using Xunit;

namespace Bastion.Server.Application.Tests.Rooms;

public class TestRoomMatchService
{
    private const int HostAccountId = 7;
    private const string HostNickname = "marta_muros";
    private const string HostToken = "marta-token";
    private const int GuestAccountId = 8;
    private const string GuestNickname = "pedro_peon";
    private const string GuestToken = "pedro-token";
    private const string Code = "K7MTQ3";
    private const byte RoomWallsPerPlayer = 3;
    private const byte FiveMinuteClock = 5;
    private const int FirstMatchId = 100;

    private static readonly DateTime _now = new DateTime(2026, 11, 5, 18, 0, 0, DateTimeKind.Utc);

    private readonly FakeSessionVerifier _sessionVerifier = new FakeSessionVerifier();
    private readonly FakeMatchmakingNotifier _matchmakingNotifier = new FakeMatchmakingNotifier();
    private readonly FakeMatchRepository _matchRepository = new FakeMatchRepository();
    private readonly FakeRoomCodeGenerator _codeGenerator = new FakeRoomCodeGenerator();
    private readonly FakeRoomRepository _roomRepository = new FakeRoomRepository();
    private readonly FakeTurnOrderShuffler _turnOrderShuffler = new FakeTurnOrderShuffler();
    private readonly RoomRegistry _roomRegistry;
    private readonly PrivateRoomService _privateRoomService;
    private readonly RoomMatchService _roomMatchService;

    public TestRoomMatchService()
    {
        _roomRegistry = new RoomRegistry(_codeGenerator);
        var playerActivity = new PlayerActivity(_matchmakingNotifier, new MatchmakingQueue(), _roomRegistry);
        var playerVerifier = new RoomPlayerVerifier(_sessionVerifier, _matchRepository, playerActivity);
        var roomKeeper = new RoomKeeper(_roomRegistry, _roomRepository, new FakeRoomNotifier());
        var settingsValidator = new RoomSettingsValidator(_matchRepository);
        _privateRoomService = new PrivateRoomService(playerVerifier, settingsValidator, roomKeeper);
        var matchCreator = new PrivateMatchCreator(_matchRepository, _roomRepository, _turnOrderShuffler);
        var matchStarter = new RoomMatchStarter(_roomRegistry, matchCreator, _matchmakingNotifier);
        _roomMatchService = new RoomMatchService(playerVerifier, matchStarter);
        AddPlayer(HostToken, HostAccountId, HostNickname);
        AddPlayer(GuestToken, GuestAccountId, GuestNickname);
        _codeGenerator.Codes.Enqueue(Code);
    }

    [Fact]
    public void StartMatch_FreeSeat_RejectsNotReadyToStart()
    {
        OpenRoom();

        RoomRejection rejection = CatchRejection(() => _roomMatchService.StartMatch(HostToken, _now));

        Assert.Equal(RoomRejection.NotReadyToStart, rejection);
    }

    [Fact]
    public void StartMatch_GuestNotReady_RejectsNotReadyToStart()
    {
        OpenRoom();
        _privateRoomService.JoinRoom(GuestToken, Code, _now);

        RoomRejection rejection = CatchRejection(() => _roomMatchService.StartMatch(HostToken, _now));

        Assert.Equal(RoomRejection.NotReadyToStart, rejection);
    }

    [Fact]
    public void StartMatch_Guest_RejectsNotHost()
    {
        FillRoom();

        RoomRejection rejection = CatchRejection(() => _roomMatchService.StartMatch(GuestToken, _now));

        Assert.Equal(RoomRejection.NotHost, rejection);
    }

    [Fact]
    public void StartMatch_ReadyRoom_SavesPrivateMatchForRoom()
    {
        int roomId = FillRoom();

        _roomMatchService.StartMatch(HostToken, _now);

        Assert.Equal(MatchType.Private, _roomRepository.SavedMatchesByRoomId[roomId].MatchType);
    }

    [Fact]
    public void StartMatch_ReadyRoom_GivesWallsOfRoomToEveryPlayer()
    {
        int roomId = FillRoom();

        _roomMatchService.StartMatch(HostToken, _now);

        Match match = _roomRepository.SavedMatchesByRoomId[roomId];
        Assert.True(match.Participations.All(participation => participation.RemainingWalls == RoomWallsPerPlayer));
    }

    [Fact]
    public void StartMatch_ReadyRoom_ReturnsSavedMatchId()
    {
        FillRoom();

        int matchId = _roomMatchService.StartMatch(HostToken, _now);

        Assert.Equal(FirstMatchId, matchId);
    }

    [Fact]
    public void StartMatch_ShuffledTurnOrder_GivesFirstTurnToGuest()
    {
        FillRoom();
        _turnOrderShuffler.ShouldReverseOrder = true;

        _roomMatchService.StartMatch(HostToken, _now);

        CreatedMatchParticipant firstPlayer = _matchmakingNotifier.CreatedMatches.Single().Participants[0];
        Assert.Equal((GuestAccountId, Match.FirstTurnOrder), (firstPlayer.Player.AccountId, firstPlayer.TurnOrder));
    }

    [Fact]
    public void StartMatch_ReadyRoom_NotifiesMatchWithRoomWalls()
    {
        FillRoom();

        _roomMatchService.StartMatch(HostToken, _now);

        Assert.Equal(RoomWallsPerPlayer, _matchmakingNotifier.CreatedMatches.Single().Format.WallsPerPlayer);
    }

    [Fact]
    public void StartMatch_ReadyRoom_FreesMembersFromRoom()
    {
        FillRoom();

        _roomMatchService.StartMatch(HostToken, _now);

        Assert.False(_roomRegistry.IsInRoom(GuestAccountId));
    }

    [Fact]
    public void StartMatch_MatchCannotBeSaved_KeepsRoomOpen()
    {
        FillRoom();
        _roomRepository.MatchSaveFailure = new RoomRepositoryException();

        Record.Exception(() => _roomMatchService.StartMatch(HostToken, _now));

        Assert.True(_roomRegistry.IsInRoom(GuestAccountId));
    }

    [Fact]
    public void JoinRoom_RoomAlreadyInMatch_RejectsMatchStarted()
    {
        _roomRepository.CodesInMatch.Add(Code);

        RoomRejection rejection = CatchRejection(() => _privateRoomService.JoinRoom(GuestToken, Code, _now));

        Assert.Equal(RoomRejection.MatchStarted, rejection);
    }

    private static RoomRejection CatchRejection(Action request)
    {
        return Assert.Throws<RoomRejectedException>(request).Rejection;
    }

    private int OpenRoom()
    {
        var settings = new RequestedRoomSettings(GameModeCode.Classic, RoomWallsPerPlayer, FiveMinuteClock);
        return _privateRoomService.CreateRoom(HostToken, settings, _now).Identity.RoomId;
    }

    private int FillRoom()
    {
        int roomId = OpenRoom();
        _privateRoomService.JoinRoom(GuestToken, Code, _now);
        _privateRoomService.SetReady(GuestToken, true, _now);
        return roomId;
    }

    private void AddPlayer(string sessionToken, int accountId, string nickname)
    {
        _sessionVerifier.Add(sessionToken, new ConnectedPlayer(accountId, nickname));
        _matchmakingNotifier.SubscribedAccountIds.Add(accountId);
    }
}
