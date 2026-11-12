using System;
using System.Collections.Generic;
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

public sealed class TestRoomMatchService
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
    private static readonly ConnectedPlayer _host = new ConnectedPlayer(HostAccountId, HostNickname);
    private static readonly ConnectedPlayer _guest = new ConnectedPlayer(GuestAccountId, GuestNickname);

    private readonly FakeSessionVerifier _sessionVerifier = new FakeSessionVerifier();
    private readonly FakeMatchmakingNotifier _matchmakingNotifier = new FakeMatchmakingNotifier();
    private readonly MatchmakingQueue _matchmakingQueue = new MatchmakingQueue();
    private readonly FakeMatchRepository _matchRepository = new FakeMatchRepository();
    private readonly FakeRoomCodeGenerator _codeGenerator = new FakeRoomCodeGenerator();
    private readonly FakeRoomRepository _roomRepository = new FakeRoomRepository();
    private readonly FakeRoomNotifier _roomNotifier = new FakeRoomNotifier();
    private readonly FakeTurnOrderShuffler _turnOrderShuffler = new FakeTurnOrderShuffler();
    private readonly RoomRegistry _roomRegistry;
    private readonly PrivateRoomService _privateRoomService;
    private readonly RoomMatchService _roomMatchService;

    public TestRoomMatchService()
    {
        _roomRegistry = new RoomRegistry(_codeGenerator);
        var playerActivity = new PlayerActivity(_matchmakingNotifier, _matchmakingQueue, _roomRegistry);
        var playerVerifier = new RoomPlayerVerifier(_sessionVerifier, _matchRepository, playerActivity);
        var roomKeeper = new RoomKeeper(_roomRegistry, _roomRepository, _roomNotifier);
        var settingsValidator = new RoomSettingsValidator(_matchRepository);
        _privateRoomService = new PrivateRoomService(playerVerifier, settingsValidator, roomKeeper);
        var matchCreator = new PrivateMatchCreator(_matchRepository, _roomRepository, _turnOrderShuffler);
        var matchStarter = new RoomMatchStarter(_roomRegistry, matchCreator, _matchmakingNotifier);
        _roomMatchService = new RoomMatchService(playerVerifier, matchStarter);
    }

    [Fact]
    public void StartMatch_FreeSeat_RejectsNotReadyToStart()
    {
        OpenRoom();
        Action start = () => _roomMatchService.StartMatch(HostToken, _now);

        Exception? exception = Record.Exception(start);

        var rejection = exception as RoomRejectedException;
        Assert.Equal(RoomRejection.NotReadyToStart, rejection?.Rejection);
    }

    [Fact]
    public void StartMatch_GuestNotReady_RejectsNotReadyToStart()
    {
        OpenRoom();
        AddPlayer(GuestToken, _guest);
        _privateRoomService.JoinRoom(GuestToken, Code, _now);
        Action start = () => _roomMatchService.StartMatch(HostToken, _now);

        Exception? exception = Record.Exception(start);

        var rejection = exception as RoomRejectedException;
        Assert.Equal(RoomRejection.NotReadyToStart, rejection?.Rejection);
    }

    [Fact]
    public void StartMatch_Guest_RejectsNotHost()
    {
        FillRoom();
        Action start = () => _roomMatchService.StartMatch(GuestToken, _now);

        Exception? exception = Record.Exception(start);

        var rejection = exception as RoomRejectedException;
        Assert.Equal(RoomRejection.NotHost, rejection?.Rejection);
    }

    [Fact]
    public void StartMatch_ReadyRoom_SavesPrivateMatchForRoom()
    {
        int roomId = FillRoom();

        _roomMatchService.StartMatch(HostToken, _now);

        Match savedMatch = _roomRepository.SavedMatchesByRoomId[roomId];
        Assert.Equal(MatchType.Private, savedMatch.MatchType);
    }

    [Fact]
    public void StartMatch_ReadyRoom_GivesWallsOfRoomToEveryPlayer()
    {
        int roomId = FillRoom();
        byte[] expectedWalls = { RoomWallsPerPlayer, RoomWallsPerPlayer };

        _roomMatchService.StartMatch(HostToken, _now);

        Match savedMatch = _roomRepository.SavedMatchesByRoomId[roomId];
        IEnumerable<byte> remainingWalls = savedMatch.Participations.Select(participation => participation.RemainingWalls);
        Assert.Equal(expectedWalls, remainingWalls);
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
        (int AccountId, byte TurnOrder) expectedFirstPlayer = (GuestAccountId, Match.FirstTurnOrder);

        _roomMatchService.StartMatch(HostToken, _now);

        CreatedMatch createdMatch = _matchmakingNotifier.CreatedMatches.Single();
        CreatedMatchParticipant firstPlayer = createdMatch.Participants[0];
        Assert.Equal(expectedFirstPlayer, (firstPlayer.Player.AccountId, firstPlayer.TurnOrder));
    }

    [Fact]
    public void StartMatch_ReadyRoom_NotifiesMatchWithRoomWalls()
    {
        FillRoom();

        _roomMatchService.StartMatch(HostToken, _now);

        CreatedMatch createdMatch = _matchmakingNotifier.CreatedMatches.Single();
        Assert.Equal(RoomWallsPerPlayer, createdMatch.Format.WallsPerPlayer);
    }

    [Fact]
    public void StartMatch_ReadyRoom_FreesMembersFromRoom()
    {
        FillRoom();

        _roomMatchService.StartMatch(HostToken, _now);

        bool isGuestInRoom = _roomRegistry.IsInRoom(GuestAccountId);
        Assert.False(isGuestInRoom);
    }

    [Fact]
    public void StartMatch_MatchCannotBeSaved_KeepsRoomOpen()
    {
        FillRoom();
        _roomRepository.MatchSaveFailure = new RoomRepositoryException();
        Action start = () => _roomMatchService.StartMatch(HostToken, _now);

        Record.Exception(start);

        bool isGuestInRoom = _roomRegistry.IsInRoom(GuestAccountId);
        Assert.True(isGuestInRoom);
    }

    private int OpenRoom()
    {
        AddPlayer(HostToken, _host);
        _codeGenerator.Codes.Enqueue(Code);
        var settings = new RequestedRoomSettings(GameModeCode.Classic, RoomWallsPerPlayer, FiveMinuteClock);
        RoomSnapshot room = _privateRoomService.CreateRoom(HostToken, settings, _now);
        return room.Identity.RoomId;
    }

    private void AddPlayer(string sessionToken, ConnectedPlayer player)
    {
        _sessionVerifier.Add(sessionToken, player);
        _matchmakingNotifier.SubscribedAccountIds.Add(player.AccountId);
    }

    private int FillRoom()
    {
        int roomId = OpenRoom();
        AddPlayer(GuestToken, _guest);
        _privateRoomService.JoinRoom(GuestToken, Code, _now);
        _privateRoomService.SetReady(GuestToken, isReady: true, _now);
        return roomId;
    }
}
