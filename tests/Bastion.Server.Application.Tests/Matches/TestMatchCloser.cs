using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.GameModes;
using Bastion.Domain.Matches;
using Bastion.Domain.Statistics;
using Bastion.Server.Application.Matches;
using Bastion.Server.Application.Tests.Fakes;
using Bastion.Server.Application.Tests.GameModes;
using Xunit;

namespace Bastion.Server.Application.Tests.Matches;

public sealed class TestMatchCloser
{
    private const int LeaverAccountId = 12;
    private const int WinnerAccountId = 13;
    private const int ThirdAccountId = 14;
    private const int FourthAccountId = 15;
    private const short StartingElo = 1000;
    private const short WinnerFinalElo = 1016;
    private const byte ClockMinutes = 5;
    private const int TwoStatistics = 2;
    private const int OneMatch = 1;
    private const int SevenMinutesInSeconds = 420;

    private static readonly DateTime _startedAt = new DateTime(2026, 11, 19, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _endedAt = new DateTime(2026, 11, 19, 18, 7, 0, DateTimeKind.Utc);

    private readonly FakeMatchRepository _matchRepository = new FakeMatchRepository();
    private readonly FakeMatchEndNotifier _matchEndNotifier = new FakeMatchEndNotifier();
    private readonly MatchCloser _matchCloser;

    public TestMatchCloser()
    {
        _matchCloser = new MatchCloser(_matchRepository, _matchEndNotifier);
    }

    [Fact]
    public void CloseWithoutMove_RankedMatch_SavesWinnerFinalElo()
    {
        Match match = StartAbandonedRankedMatch();

        _matchCloser.CloseWithoutMove(match);

        Participation winner = SaveAndFindParticipation(WinnerAccountId);
        Assert.Equal(WinnerFinalElo, winner.FinalElo);
    }

    [Fact]
    public void CloseWithoutMove_NoStoredStatistics_StartsBoth()
    {
        Match match = StartAbandonedRankedMatch();

        _matchCloser.CloseWithoutMove(match);

        MatchClosure closure = _matchRepository.ClosuresSavedWithoutMove.Single();
        Assert.Equal(TwoStatistics, closure.StartedStatistics.Count);
    }

    [Fact]
    public void CloseWithoutMove_StoredStatistic_RecordsTheMatchInIt()
    {
        ModeStatistic stored = AddStoredStatistic(WinnerAccountId);
        Match match = StartAbandonedRankedMatch();

        _matchCloser.CloseWithoutMove(match);

        Assert.Equal(OneMatch, stored.MatchesPlayed);
    }

    [Fact]
    public void CloseWithoutMove_StoredStatistic_StartsOnlyTheMissingOne()
    {
        AddStoredStatistic(WinnerAccountId);
        Match match = StartAbandonedRankedMatch();

        _matchCloser.CloseWithoutMove(match);

        MatchClosure closure = _matchRepository.ClosuresSavedWithoutMove.Single();
        Assert.Single(closure.StartedStatistics);
    }

    [Fact]
    public void CloseWithoutMove_RankedMatch_CountsLeaverAbandonment()
    {
        ModeStatistic leaverStatistic = AddStoredStatistic(LeaverAccountId);
        Match match = StartAbandonedRankedMatch();

        _matchCloser.CloseWithoutMove(match);

        Assert.Equal(OneMatch, leaverStatistic.Abandonments);
    }

    [Fact]
    public void CloseWithoutMove_PrivateMatch_StartsNoStatistics()
    {
        Match match = StartPrivateMatch();
        match.Abandon(LeaverAccountId, _endedAt);

        _matchCloser.CloseWithoutMove(match);

        MatchClosure closure = _matchRepository.ClosuresSavedWithoutMove.Single();
        Assert.Empty(closure.StartedStatistics);
    }

    [Fact]
    public void CloseWithoutMove_RankedMatchWithoutInitialElo_SavesNoFinalElo()
    {
        MatchFormat format = CreateClassicFormat();
        MatchEntrant[] entrants =
        [
            new MatchEntrant(LeaverAccountId, EloRating: null),
            new MatchEntrant(WinnerAccountId, EloRating: null),
        ];
        var match = Match.StartRanked(format, entrants, _startedAt);
        match.Abandon(LeaverAccountId, _endedAt);

        _matchCloser.CloseWithoutMove(match);

        Participation winner = SaveAndFindParticipation(WinnerAccountId);
        Assert.Null(winner.FinalElo);
    }

    [Fact]
    public void CloseWithoutMove_MatchInProgress_ThrowsInvalidOperationException()
    {
        Match match = StartRankedMatch();

        Action close = () => _matchCloser.CloseWithoutMove(match);

        Assert.Throws<InvalidOperationException>(close);
    }

    [Fact]
    public void AnnounceEnd_TwoPlayers_NotifiesStandingsInTurnOrder()
    {
        Match match = StartAbandonedRankedMatch();
        byte[] expectedTurnOrders = { Match.FirstTurnOrder, Match.SecondTurnOrder };

        _matchCloser.AnnounceEnd(match, LeaverAccountId);

        NotifiedMatchEnd notified = _matchEndNotifier.NotifiedMatchEnds.Single();
        IEnumerable<byte> turnOrders = notified.Notice.Standings.Select(standing => standing.TurnOrder);
        Assert.Equal(expectedTurnOrders, turnOrders);
    }

    [Fact]
    public void AnnounceEnd_EndedSevenMinutesAfterStart_NotifiesPlayedSeconds()
    {
        Match match = StartAbandonedRankedMatch();

        _matchCloser.AnnounceEnd(match, LeaverAccountId);

        NotifiedMatchEnd notified = _matchEndNotifier.NotifiedMatchEnds.Single();
        Assert.Equal(SevenMinutesInSeconds, notified.Notice.PlayedSeconds);
    }

    [Fact]
    public void AnnounceEnd_RankedMatch_NotifiesRankedMatch()
    {
        Match match = StartAbandonedRankedMatch();

        _matchCloser.AnnounceEnd(match, LeaverAccountId);

        NotifiedMatchEnd notified = _matchEndNotifier.NotifiedMatchEnds.Single();
        Assert.True(notified.Notice.IsRanked);
    }

    [Fact]
    public void AnnounceDeparture_FourPlayersLeaverHadTurn_NotifiesNextTurn()
    {
        Match match = StartFourPlayerMatch();
        match.Abandon(LeaverAccountId, _endedAt);

        _matchCloser.AnnounceDeparture(match, LeaverAccountId, _endedAt);

        NotifiedParticipationEnd notified = _matchEndNotifier.NotifiedParticipationEnds.Single();
        Assert.Equal(Match.SecondTurnOrder, notified.Notice.NextTurnOrder);
    }

    [Fact]
    public void AnnounceDeparture_FourPlayersLeaver_NotifiesLeaverTurnOrder()
    {
        Match match = StartFourPlayerMatch();
        match.Abandon(LeaverAccountId, _endedAt);

        _matchCloser.AnnounceDeparture(match, LeaverAccountId, _endedAt);

        NotifiedParticipationEnd notified = _matchEndNotifier.NotifiedParticipationEnds.Single();
        Assert.Equal(Match.FirstTurnOrder, notified.Notice.DepartingTurnOrder);
    }

    [Fact]
    public void ExpireClock_TwoPlayers_SavesClosureWithoutMove()
    {
        Match match = StartRankedMatch();

        _matchCloser.ExpireClock(match, _endedAt);

        Assert.Single(_matchRepository.ClosuresSavedWithoutMove);
    }

    [Fact]
    public void ExpireClock_TwoPlayers_GivesTheRivalTheWin()
    {
        Match match = StartRankedMatch();

        _matchCloser.ExpireClock(match, _endedAt);

        Participation winner = SaveAndFindParticipation(WinnerAccountId);
        Assert.Equal(ParticipationResult.Won, winner.Result);
    }

    [Fact]
    public void ExpireClock_TwoPlayers_AnnouncesTimeoutDecidedByTimedOutPlayer()
    {
        Match match = StartRankedMatch();

        _matchCloser.ExpireClock(match, _endedAt);

        NotifiedMatchEnd notified = _matchEndNotifier.NotifiedMatchEnds.Single();
        Assert.Equal(Match.FirstTurnOrder, notified.Notice.DecidingTurnOrder);
    }

    [Fact]
    public void ExpireClock_FourPlayers_SavesDeparture()
    {
        Match match = StartFourPlayerMatch();

        _matchCloser.ExpireClock(match, _endedAt);

        Assert.Single(_matchRepository.MatchesSavedWithDeparture);
    }

    [Fact]
    public void ExpireClock_FourPlayers_AnnouncesTimeoutDeparture()
    {
        Match match = StartFourPlayerMatch();

        _matchCloser.ExpireClock(match, _endedAt);

        NotifiedParticipationEnd notified = _matchEndNotifier.NotifiedParticipationEnds.Single();
        Assert.Equal(MatchEndReason.Timeout, notified.Notice.EndReason);
    }

    [Fact]
    public void ExpireClock_ClockLeft_ThrowsInvalidOperationException()
    {
        Match match = StartRankedMatch();

        Action expireClock = () => _matchCloser.ExpireClock(match, _startedAt);

        Assert.Throws<InvalidOperationException>(expireClock);
    }

    private Participation SaveAndFindParticipation(int accountId)
    {
        MatchClosure closure = _matchRepository.ClosuresSavedWithoutMove.Single();
        return closure.Match.GetParticipation(accountId);
    }

    private ModeStatistic AddStoredStatistic(int accountId)
    {
        GameMode classicMode = GameModeSampleFactory.CreateClassic();
        var statistic = ModeStatistic.StartFor(accountId, classicMode.GameModeId);
        _matchRepository.StoredModeStatistics.Add(statistic);
        return statistic;
    }

    private static Match StartAbandonedRankedMatch()
    {
        Match match = StartRankedMatch();
        match.Abandon(LeaverAccountId, _endedAt);
        return match;
    }

    private static Match StartRankedMatch()
    {
        MatchFormat format = CreateClassicFormat();
        MatchEntrant[] entrants = CreateTwoEntrants();
        return Match.StartRanked(format, entrants, _startedAt);
    }

    private static Match StartPrivateMatch()
    {
        MatchFormat format = CreateClassicFormat();
        MatchEntrant[] entrants = CreateTwoEntrants();
        return Match.StartPrivate(format, entrants, _startedAt);
    }

    private static MatchFormat CreateClassicFormat()
    {
        GameMode classicMode = GameModeSampleFactory.CreateClassic();
        return new MatchFormat(classicMode, ClockMinutes);
    }

    private static MatchEntrant[] CreateTwoEntrants()
    {
        return
        [
            new MatchEntrant(LeaverAccountId, StartingElo),
            new MatchEntrant(WinnerAccountId, StartingElo),
        ];
    }

    private static Match StartFourPlayerMatch()
    {
        GameMode fourPlayerMode = GameModeSampleFactory.CreateFourPlayers();
        var format = new MatchFormat(fourPlayerMode, ClockMinutes);
        MatchEntrant[] entrants =
        [
            new MatchEntrant(LeaverAccountId, EloRating: null),
            new MatchEntrant(WinnerAccountId, EloRating: null),
            new MatchEntrant(ThirdAccountId, EloRating: null),
            new MatchEntrant(FourthAccountId, EloRating: null),
        ];
        return Match.StartPrivate(format, entrants, _startedAt);
    }
}
