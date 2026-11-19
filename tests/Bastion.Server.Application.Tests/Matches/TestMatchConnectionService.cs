using System;
using System.Linq;
using Bastion.Domain.Common;
using Bastion.Domain.GameModes;
using Bastion.Domain.Matches;
using Bastion.Server.Application.Matches;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Application.Tests.Fakes;
using Bastion.Server.Application.Tests.GameModes;
using Xunit;

namespace Bastion.Server.Application.Tests.Matches;

public sealed class TestMatchConnectionService
{
    private const int MatchId = 41;
    private const int UnknownMatchId = 99;
    private const int FirstAccountId = 12;
    private const int SecondAccountId = 13;
    private const int OutsiderAccountId = 5;
    private const string FirstNickname = "marta_muros";
    private const string SecondNickname = "pedro_peon";
    private const string OutsiderNickname = "nico_nuevo";
    private const string FirstToken = "first-token";
    private const string SecondToken = "second-token";
    private const string OutsiderToken = "outsider-token";
    private const short StartingElo = 1000;
    private const byte ClockMinutes = 5;

    private static readonly DateTime _startedAt = new DateTime(2026, 11, 19, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _disconnectedAt = new DateTime(2026, 11, 19, 18, 1, 0, DateTimeKind.Utc);
    private static readonly DateTime _halfMinuteLater = _disconnectedAt.AddSeconds(30);
    private static readonly DateTime _fullDelayLater = _disconnectedAt + MatchConnectionPolicy.ReturnDelay;
    private static readonly DateTime _afterClockRanOut = new DateTime(2026, 11, 19, 18, 6, 0, DateTimeKind.Utc);
    private static readonly ConnectedPlayer _firstPlayer = new ConnectedPlayer(FirstAccountId, FirstNickname);
    private static readonly ConnectedPlayer _secondPlayer = new ConnectedPlayer(SecondAccountId, SecondNickname);
    private static readonly ConnectedPlayer _outsider = new ConnectedPlayer(OutsiderAccountId, OutsiderNickname);

    private readonly FakeSessionVerifier _sessionVerifier = new FakeSessionVerifier();
    private readonly FakeMatchRepository _matchRepository = new FakeMatchRepository();
    private readonly FakeMatchEndNotifier _matchEndNotifier = new FakeMatchEndNotifier();
    private readonly FakeMatchConnectionNotifier _connectionNotifier = new FakeMatchConnectionNotifier();
    private readonly MatchConnectionService _connectionService;

    public TestMatchConnectionService()
    {
        var matchCloser = new MatchCloser(_matchRepository, _matchEndNotifier);
        var followUp = new MatchConnectionFollowUp(matchCloser, _connectionNotifier, new MatchGate());
        _connectionService = new MatchConnectionService(_sessionVerifier, _matchRepository, followUp);
        _sessionVerifier.Add(FirstToken, _firstPlayer);
        _sessionVerifier.Add(SecondToken, _secondPlayer);
        _sessionVerifier.Add(OutsiderToken, _outsider);
        _matchRepository.NicknamesByAccountId[FirstAccountId] = FirstNickname;
        _matchRepository.NicknamesByAccountId[SecondAccountId] = SecondNickname;
    }

    [Fact]
    public void MarkDisconnected_PlayerInMatch_SavesTheDisconnection()
    {
        AddMatch();

        _connectionService.MarkDisconnected(SecondAccountId, _disconnectedAt);

        Match savedMatch = _matchRepository.MatchesSavedWithConnection.Single();
        Assert.False(savedMatch.GetParticipation(SecondAccountId).IsConnected);
    }

    [Fact]
    public void MarkDisconnected_PlayerInMatch_TellsTheRivalWhoDropped()
    {
        AddMatch();

        _connectionService.MarkDisconnected(SecondAccountId, _disconnectedAt);

        OpponentDisconnection disconnection = _connectionNotifier.Disconnections.Single();
        Assert.Equal(Match.SecondTurnOrder, disconnection.TurnOrder);
    }

    [Fact]
    public void MarkDisconnected_PlayerInMatch_NotifiesOnlyTheOtherPlayers()
    {
        AddMatch();
        int[] expectedRecipients = { FirstAccountId };

        _connectionService.MarkDisconnected(SecondAccountId, _disconnectedAt);

        Assert.Equal(expectedRecipients, _connectionNotifier.DisconnectionRecipients);
    }

    [Fact]
    public void MarkDisconnected_PlayerWithoutMatch_SavesNothing()
    {
        _connectionService.MarkDisconnected(OutsiderAccountId, _disconnectedAt);

        Assert.Empty(_matchRepository.MatchesSavedWithConnection);
    }

    [Fact]
    public void MarkDisconnected_AlreadyDisconnected_NotifiesNothing()
    {
        Match match = AddMatch();
        match.Disconnect(SecondAccountId, _disconnectedAt);

        _connectionService.MarkDisconnected(SecondAccountId, _halfMinuteLater);

        Assert.Empty(_connectionNotifier.Disconnections);
    }

    [Fact]
    public void FindOngoingMatch_PlayerInMatch_ReturnsTheMatch()
    {
        Match match = AddMatch();

        Lookup<MatchSnapshot> foundSnapshot = _connectionService.FindOngoingMatch(SecondToken, _halfMinuteLater);

        Assert.Same(match, foundSnapshot.GetValue().Match);
    }

    [Fact]
    public void FindOngoingMatch_PlayerDisconnected_KeepsThePlayerDisconnected()
    {
        Match match = AddMatch();
        match.Disconnect(SecondAccountId, _disconnectedAt);

        _connectionService.FindOngoingMatch(SecondToken, _halfMinuteLater);

        Assert.False(match.GetParticipation(SecondAccountId).IsConnected);
    }

    [Fact]
    public void FindOngoingMatch_PlayerWithoutMatch_ReturnsNothing()
    {
        Lookup<MatchSnapshot> foundSnapshot = _connectionService.FindOngoingMatch(OutsiderToken, _halfMinuteLater);

        Assert.False(foundSnapshot.IsFound);
    }

    [Fact]
    public void RejoinMatch_DisconnectedPlayer_MarksThePlayerConnected()
    {
        Match match = AddMatch();
        match.Disconnect(SecondAccountId, _disconnectedAt);

        _connectionService.RejoinMatch(SecondToken, MatchId, _halfMinuteLater);

        Assert.True(match.GetParticipation(SecondAccountId).IsConnected);
    }

    [Fact]
    public void RejoinMatch_DisconnectedPlayer_TellsTheRivalWhoCameBack()
    {
        Match match = AddMatch();
        match.Disconnect(SecondAccountId, _disconnectedAt);

        _connectionService.RejoinMatch(SecondToken, MatchId, _halfMinuteLater);

        Assert.Equal(Match.SecondTurnOrder, _connectionNotifier.ReconnectedTurnOrders.Single());
    }

    [Fact]
    public void RejoinMatch_ConnectedPlayer_NotifiesNothing()
    {
        AddMatch();

        _connectionService.RejoinMatch(SecondToken, MatchId, _halfMinuteLater);

        Assert.Empty(_connectionNotifier.ReconnectedTurnOrders);
    }

    [Fact]
    public void RejoinMatch_ClockRanOutWhileAway_ReturnsTheEnding()
    {
        Match match = AddMatch();
        match.Disconnect(FirstAccountId, _disconnectedAt);

        MatchSnapshot snapshot = _connectionService.RejoinMatch(FirstToken, MatchId, _afterClockRanOut);

        Assert.Equal(MatchEndReason.Timeout, snapshot.Ending?.EndReason);
    }

    [Fact]
    public void RejoinMatch_TakenOutWhileAway_ReturnsTheAbsentPlayerAsDeciding()
    {
        Match match = AddMatch();
        match.Disconnect(SecondAccountId, _disconnectedAt);
        match.TakeOutAbsentPlayer(_fullDelayLater);

        MatchSnapshot snapshot = _connectionService.RejoinMatch(SecondToken, MatchId, _fullDelayLater);

        Assert.Equal(Match.SecondTurnOrder, snapshot.Ending?.DecidingTurnOrder);
    }

    [Fact]
    public void RejoinMatch_Outsider_ThrowsMatchNotJoinedException()
    {
        AddMatch();

        Action rejoinMatch = () => _connectionService.RejoinMatch(OutsiderToken, MatchId, _halfMinuteLater);

        Assert.Throws<MatchNotJoinedException>(rejoinMatch);
    }

    [Fact]
    public void RejoinMatch_UnknownMatch_ThrowsMatchNotJoinedException()
    {
        Action rejoinMatch = () => _connectionService.RejoinMatch(FirstToken, UnknownMatchId, _halfMinuteLater);

        Assert.Throws<MatchNotJoinedException>(rejoinMatch);
    }

    private Match AddMatch()
    {
        GameMode classicMode = GameModeSampleFactory.CreateClassic();
        var format = new MatchFormat(classicMode, ClockMinutes);
        MatchEntrant[] entrants =
        [
            new MatchEntrant(FirstAccountId, StartingElo),
            new MatchEntrant(SecondAccountId, StartingElo),
        ];
        var match = Match.StartRanked(format, entrants, _startedAt);
        _matchRepository.MatchesById[MatchId] = match;
        _matchRepository.UnfinishedMatchIdsByAccountId[FirstAccountId] = MatchId;
        _matchRepository.UnfinishedMatchIdsByAccountId[SecondAccountId] = MatchId;
        return match;
    }
}
