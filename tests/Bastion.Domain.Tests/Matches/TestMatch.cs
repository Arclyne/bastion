using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.GameModes;
using Bastion.Domain.Matches;
using Bastion.Domain.Tests.GameModes;
using Xunit;

namespace Bastion.Domain.Tests.Matches;

public sealed class TestMatch
{
    private const int FirstAccountId = 3;
    private const int SecondAccountId = 6;
    private const short FirstEloRating = 1055;
    private const short SecondEloRating = 1000;
    private const byte ClockMinutes = 5;
    private const int FiveMinutesInMilliseconds = 300000;
    private const string ClassicFirstSquare = "e1";
    private const string ClassicSecondSquare = "e9";
    private const string QuickSecondSquare = "d7";
    private const int OutsiderAccountId = 9;
    private const byte RoomWallsPerPlayer = 3;
    private const int ThirdAccountId = 7;
    private const int FourthAccountId = 8;
    private const byte LastOfFourPlacement = 4;
    private const byte ThirdPlacement = 3;
    private const byte FirstPlacement = 1;

    private static readonly DateTime _startedAt = new DateTime(2026, 11, 5, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _endedAt = new DateTime(2026, 11, 5, 18, 7, 0, DateTimeKind.Utc);
    private static readonly MatchEntrant _firstEntrant = new MatchEntrant(FirstAccountId, FirstEloRating);
    private static readonly MatchEntrant _secondEntrant = new MatchEntrant(SecondAccountId, SecondEloRating);
    private static readonly MatchEntrant[] _entrants = { _firstEntrant, _secondEntrant };

    [Fact]
    public void StartRanked_TwoEntrants_CreatesOneParticipationEach()
    {
        MatchFormat format = CreateClassicFormat();

        var match = Match.StartRanked(format, _entrants, _startedAt);

        Assert.Equal(_entrants.Length, match.Participations.Count);
    }

    [Fact]
    public void StartPrivate_TwoEntrants_CreatesPrivateMatch()
    {
        MatchFormat format = CreateClassicFormat();
        MatchEntrant[] entrants = CreatePrivateEntrants();

        var match = Match.StartPrivate(format, entrants, _startedAt);

        Assert.Equal(MatchType.Private, match.MatchType);
    }

    [Fact]
    public void StartPrivate_WallsChosenForRoom_GivesThoseWallsToEveryPlayer()
    {
        MatchFormat classicFormat = CreateClassicFormat();
        MatchFormat format = classicFormat with { WallsPerPlayer = RoomWallsPerPlayer };
        MatchEntrant[] entrants = CreatePrivateEntrants();
        byte[] expectedWalls = { RoomWallsPerPlayer, RoomWallsPerPlayer };

        var match = Match.StartPrivate(format, entrants, _startedAt);

        IEnumerable<byte> remainingWalls = match.Participations.Select(participation => participation.RemainingWalls);
        Assert.Equal(expectedWalls, remainingWalls);
    }

    [Fact]
    public void StartPrivate_EntrantsWithoutElo_SavesNoInitialElo()
    {
        MatchFormat format = CreateClassicFormat();
        MatchEntrant[] entrants = CreatePrivateEntrants();
        short?[] expectedElos = { null, null };

        var match = Match.StartPrivate(format, entrants, _startedAt);

        IEnumerable<short?> initialElos = match.Participations.Select(participation => participation.InitialElo);
        Assert.Equal(expectedElos, initialElos);
    }

    [Fact]
    public void StartRanked_ClassicBoard_PlacesFirstPlayerOnE1()
    {
        MatchFormat format = CreateClassicFormat();

        var match = Match.StartRanked(format, _entrants, _startedAt);

        Participation first = FindParticipation(match, FirstAccountId);
        Assert.Equal(ClassicFirstSquare, first.CurrentSquare);
    }

    [Fact]
    public void StartRanked_ClassicBoard_PlacesSecondPlayerOnE9()
    {
        MatchFormat format = CreateClassicFormat();

        var match = Match.StartRanked(format, _entrants, _startedAt);

        Participation second = FindParticipation(match, SecondAccountId);
        Assert.Equal(ClassicSecondSquare, second.CurrentSquare);
    }

    [Fact]
    public void StartRanked_QuickBoard_PlacesSecondPlayerOnD7()
    {
        GameMode quickMode = GameModeSampleFactory.CreateQuick();
        var format = new MatchFormat(quickMode, ClockMinutes);

        var match = Match.StartRanked(format, _entrants, _startedAt);

        Participation second = FindParticipation(match, SecondAccountId);
        Assert.Equal(QuickSecondSquare, second.CurrentSquare);
    }

    [Fact]
    public void StartRanked_SecondEntrant_GetsRedPawn()
    {
        MatchFormat format = CreateClassicFormat();

        var match = Match.StartRanked(format, _entrants, _startedAt);

        Participation second = FindParticipation(match, SecondAccountId);
        Assert.Equal(PawnSymbol.Red, second.PawnSymbol);
    }

    [Fact]
    public void StartRanked_ClassicMode_GivesTheModeWalls()
    {
        GameMode classicMode = GameModeSampleFactory.CreateClassic();
        var format = new MatchFormat(classicMode, ClockMinutes);
        byte expectedWalls = classicMode.WallsPerPlayer;

        var match = Match.StartRanked(format, _entrants, _startedAt);

        Participation first = FindParticipation(match, FirstAccountId);
        Assert.Equal(expectedWalls, first.RemainingWalls);
    }

    [Fact]
    public void StartRanked_FiveMinuteClock_GivesFiveMinutesInMilliseconds()
    {
        MatchFormat format = CreateClassicFormat();

        var match = Match.StartRanked(format, _entrants, _startedAt);

        Participation first = FindParticipation(match, FirstAccountId);
        Assert.Equal(FiveMinutesInMilliseconds, first.RemainingClockMilliseconds);
    }

    [Fact]
    public void StartRanked_Entrant_KeepsInitialElo()
    {
        MatchFormat format = CreateClassicFormat();

        var match = Match.StartRanked(format, _entrants, _startedAt);

        Participation first = FindParticipation(match, FirstAccountId);
        Assert.Equal(FirstEloRating, first.InitialElo);
    }

    [Fact]
    public void StartRanked_NewMatch_GivesTurnToFirstTurnOrder()
    {
        MatchFormat format = CreateClassicFormat();

        var match = Match.StartRanked(format, _entrants, _startedAt);

        Assert.Equal(Match.FirstTurnOrder, match.CurrentTurnOrder);
    }

    [Fact]
    public void StartRanked_OneEntrant_ThrowsArgumentException()
    {
        MatchFormat format = CreateClassicFormat();
        MatchEntrant[] entrants = { _firstEntrant };

        Action start = () => Match.StartRanked(format, entrants, _startedAt);

        Assert.Throws<ArgumentException>(start);
    }

    [Fact]
    public void Abandon_FirstPlayerLeaves_FirstPlayerLoses()
    {
        Match match = StartClassicMatch();

        match.Abandon(FirstAccountId, _endedAt);

        Participation first = FindParticipation(match, FirstAccountId);
        Assert.Equal(ParticipationResult.Lost, first.Result);
    }

    [Fact]
    public void Abandon_FirstPlayerLeaves_RivalWins()
    {
        Match match = StartClassicMatch();

        match.Abandon(FirstAccountId, _endedAt);

        Participation second = FindParticipation(match, SecondAccountId);
        Assert.Equal(ParticipationResult.Won, second.Result);
    }

    [Fact]
    public void Abandon_MatchInProgress_FinishesMatch()
    {
        Match match = StartClassicMatch();

        match.Abandon(SecondAccountId, _endedAt);

        Assert.Equal(MatchStatus.Finished, match.Status);
    }

    [Fact]
    public void Abandon_MatchInProgress_RecordsAbandonment()
    {
        Match match = StartClassicMatch();

        match.Abandon(SecondAccountId, _endedAt);

        Assert.Equal(MatchEndReason.Abandonment, match.EndReason);
    }

    [Fact]
    public void Abandon_MatchInProgress_RecordsEndTime()
    {
        Match match = StartClassicMatch();

        match.Abandon(SecondAccountId, _endedAt);

        Assert.Equal(_endedAt, match.EndedAt);
    }

    [Fact]
    public void Abandon_FinishedMatch_ThrowsInvalidOperationException()
    {
        Match match = StartClassicMatch();
        match.Abandon(FirstAccountId, _endedAt);

        Action abandonAgain = () => match.Abandon(SecondAccountId, _endedAt);

        Assert.Throws<InvalidOperationException>(abandonAgain);
    }

    [Fact]
    public void CanBeAbandonedBy_PlayerOfMatchInProgress_ReturnsTrue()
    {
        Match match = StartClassicMatch();

        bool canAbandon = match.CanBeAbandonedBy(FirstAccountId);

        Assert.True(canAbandon);
    }

    [Fact]
    public void CanBeAbandonedBy_Outsider_ReturnsFalse()
    {
        Match match = StartClassicMatch();

        bool canAbandon = match.CanBeAbandonedBy(OutsiderAccountId);

        Assert.False(canAbandon);
    }

    [Fact]
    public void Abandon_FourPlayersFirstLeaves_KeepsMatchGoing()
    {
        Match match = StartFourPlayerMatch();

        match.Abandon(FirstAccountId, _endedAt);

        Assert.Equal(MatchStatus.InProgress, match.Status);
    }

    [Fact]
    public void Abandon_FourPlayersFirstLeaves_GivesLeaverLastPlace()
    {
        Match match = StartFourPlayerMatch();
        (ParticipationResult? Result, MatchEndReason? EndReason, byte? Placement) expectedOutcome =
            (ParticipationResult.Lost, MatchEndReason.Abandonment, LastOfFourPlacement);

        match.Abandon(FirstAccountId, _endedAt);

        Participation leaver = FindParticipation(match, FirstAccountId);
        Assert.Equal(expectedOutcome, (leaver.Result, leaver.EndReason, leaver.Placement));
    }

    [Fact]
    public void Abandon_FourPlayersSecondLeaver_GivesThirdPlace()
    {
        Match match = StartFourPlayerMatch();
        match.Abandon(FirstAccountId, _endedAt);

        match.Abandon(ThirdAccountId, _endedAt);

        Participation secondLeaver = FindParticipation(match, ThirdAccountId);
        Assert.Equal(ThirdPlacement, secondLeaver.Placement);
    }

    [Fact]
    public void Abandon_FourPlayersLeaverHadTurn_PassesTurnToNextPlayer()
    {
        Match match = StartFourPlayerMatch();

        match.Abandon(FirstAccountId, _endedAt);

        Assert.Equal(Match.SecondTurnOrder, match.CurrentTurnOrder);
    }

    [Fact]
    public void Abandon_FourPlayersLeaverWithoutTurn_KeepsTurn()
    {
        Match match = StartFourPlayerMatch();

        match.Abandon(ThirdAccountId, _endedAt);

        Assert.Equal(Match.FirstTurnOrder, match.CurrentTurnOrder);
    }

    [Fact]
    public void Abandon_FourPlayersThreeLeave_LastPlayerWinsFirstPlace()
    {
        Match match = StartFourPlayerMatch();
        match.Abandon(FirstAccountId, _endedAt);
        match.Abandon(SecondAccountId, _endedAt);
        (ParticipationResult? Result, byte? Placement, MatchStatus Status) expectedOutcome =
            (ParticipationResult.Won, FirstPlacement, MatchStatus.Finished);

        match.Abandon(ThirdAccountId, _endedAt);

        Participation winner = FindParticipation(match, FourthAccountId);
        Assert.Equal(expectedOutcome, (winner.Result, winner.Placement, match.Status));
    }

    private static MatchFormat CreateClassicFormat()
    {
        GameMode classicMode = GameModeSampleFactory.CreateClassic();
        return new MatchFormat(classicMode, ClockMinutes);
    }

    private static MatchEntrant[] CreatePrivateEntrants()
    {
        MatchEntrant first = CreateUnrankedEntrant(FirstAccountId);
        MatchEntrant second = CreateUnrankedEntrant(SecondAccountId);
        return new MatchEntrant[] { first, second };
    }

    private static MatchEntrant CreateUnrankedEntrant(int accountId)
    {
        return new MatchEntrant(accountId, EloRating: null);
    }

    private static Participation FindParticipation(Match match, int accountId)
    {
        return match.Participations.First(participation => participation.AccountId == accountId);
    }

    private static Match StartClassicMatch()
    {
        MatchFormat format = CreateClassicFormat();
        return Match.StartRanked(format, _entrants, _startedAt);
    }

    private static Match StartFourPlayerMatch()
    {
        GameMode fourPlayerMode = GameModeSampleFactory.CreateFourPlayers();
        var format = new MatchFormat(fourPlayerMode, ClockMinutes);
        MatchEntrant first = CreateUnrankedEntrant(FirstAccountId);
        MatchEntrant second = CreateUnrankedEntrant(SecondAccountId);
        MatchEntrant third = CreateUnrankedEntrant(ThirdAccountId);
        MatchEntrant fourth = CreateUnrankedEntrant(FourthAccountId);
        MatchEntrant[] entrants = { first, second, third, fourth };
        return Match.StartPrivate(format, entrants, _startedAt);
    }
}
