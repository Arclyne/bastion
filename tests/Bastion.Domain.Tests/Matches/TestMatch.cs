using System;
using System.Linq;
using Bastion.Domain.GameModes;
using Bastion.Domain.Matches;
using Xunit;

namespace Bastion.Domain.Tests.Matches;

public class TestMatch
{
    private const int FirstAccountId = 3;
    private const int SecondAccountId = 6;
    private const short FirstEloRating = 1055;
    private const short SecondEloRating = 1000;
    private const byte ClockMinutes = 5;
    private const int FiveMinutesInMilliseconds = 300000;
    private const byte ClassicBoardSize = 9;
    private const byte ClassicWallsPerPlayer = 10;
    private const byte QuickBoardSize = 7;
    private const byte QuickWallsPerPlayer = 6;
    private const byte TwoPlayers = 2;
    private const string ClassicFirstSquare = "e1";
    private const string ClassicSecondSquare = "e9";
    private const string QuickSecondSquare = "d7";
    private const int OutsiderAccountId = 9;

    private static readonly DateTime _startedAt = new DateTime(2026, 11, 5, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _endedAt = new DateTime(2026, 11, 5, 18, 7, 0, DateTimeKind.Utc);
    private static readonly MatchEntrant[] _entrants = new MatchEntrant[]
    {
        new MatchEntrant(FirstAccountId, FirstEloRating),
        new MatchEntrant(SecondAccountId, SecondEloRating),
    };

    [Fact]
    public void StartRanked_TwoEntrants_CreatesOneParticipationEach()
    {
        MatchFormat format = CreateClassicFormat();

        var match = Match.StartRanked(format, _entrants, _startedAt);

        Assert.Equal(_entrants.Length, match.Participations.Count);
    }

    [Fact]
    public void StartRanked_ClassicBoard_PlacesFirstPlayerOnE1()
    {
        MatchFormat format = CreateClassicFormat();

        var match = Match.StartRanked(format, _entrants, _startedAt);

        Assert.Equal(ClassicFirstSquare, FindParticipation(match, FirstAccountId).CurrentSquare);
    }

    [Fact]
    public void StartRanked_ClassicBoard_PlacesSecondPlayerOnE9()
    {
        MatchFormat format = CreateClassicFormat();

        var match = Match.StartRanked(format, _entrants, _startedAt);

        Assert.Equal(ClassicSecondSquare, FindParticipation(match, SecondAccountId).CurrentSquare);
    }

    [Fact]
    public void StartRanked_QuickBoard_PlacesSecondPlayerOnD7()
    {
        MatchFormat format = CreateFormat(GameModeCode.Quick, QuickBoardSize, QuickWallsPerPlayer);

        var match = Match.StartRanked(format, _entrants, _startedAt);

        Assert.Equal(QuickSecondSquare, FindParticipation(match, SecondAccountId).CurrentSquare);
    }

    [Fact]
    public void StartRanked_SecondEntrant_GetsRedPawn()
    {
        MatchFormat format = CreateClassicFormat();

        var match = Match.StartRanked(format, _entrants, _startedAt);

        Assert.Equal(PawnSymbol.Red, FindParticipation(match, SecondAccountId).PawnSymbol);
    }

    [Fact]
    public void StartRanked_ClassicMode_GivesTheModeWalls()
    {
        MatchFormat format = CreateClassicFormat();

        var match = Match.StartRanked(format, _entrants, _startedAt);

        Assert.Equal(ClassicWallsPerPlayer, FindParticipation(match, FirstAccountId).RemainingWalls);
    }

    [Fact]
    public void StartRanked_FiveMinuteClock_GivesFiveMinutesInMilliseconds()
    {
        MatchFormat format = CreateClassicFormat();

        var match = Match.StartRanked(format, _entrants, _startedAt);

        Assert.Equal(FiveMinutesInMilliseconds, FindParticipation(match, FirstAccountId).RemainingClockMilliseconds);
    }

    [Fact]
    public void StartRanked_Entrant_KeepsInitialElo()
    {
        MatchFormat format = CreateClassicFormat();

        var match = Match.StartRanked(format, _entrants, _startedAt);

        Assert.Equal(FirstEloRating, FindParticipation(match, FirstAccountId).InitialElo);
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
        MatchEntrant[] entrants = _entrants.Take(1).ToArray();

        Assert.Throws<ArgumentException>(() => Match.StartRanked(format, entrants, _startedAt));
    }

    [Fact]
    public void EndByAbandonment_FirstPlayerLeaves_FirstPlayerLoses()
    {
        Match match = StartClassicMatch();

        match.EndByAbandonment(FirstAccountId, _endedAt);

        Assert.Equal(ParticipationResult.Lost, FindParticipation(match, FirstAccountId).Result);
    }

    [Fact]
    public void EndByAbandonment_FirstPlayerLeaves_RivalWins()
    {
        Match match = StartClassicMatch();

        match.EndByAbandonment(FirstAccountId, _endedAt);

        Assert.Equal(ParticipationResult.Won, FindParticipation(match, SecondAccountId).Result);
    }

    [Fact]
    public void EndByAbandonment_MatchInProgress_FinishesMatch()
    {
        Match match = StartClassicMatch();

        match.EndByAbandonment(SecondAccountId, _endedAt);

        Assert.Equal(MatchStatus.Finished, match.Status);
    }

    [Fact]
    public void EndByAbandonment_MatchInProgress_RecordsAbandonment()
    {
        Match match = StartClassicMatch();

        match.EndByAbandonment(SecondAccountId, _endedAt);

        Assert.Equal(MatchEndReason.Abandonment, match.EndReason);
    }

    [Fact]
    public void EndByAbandonment_MatchInProgress_RecordsEndTime()
    {
        Match match = StartClassicMatch();

        match.EndByAbandonment(SecondAccountId, _endedAt);

        Assert.Equal(_endedAt, match.EndedAt);
    }

    [Fact]
    public void EndByAbandonment_FinishedMatch_ThrowsInvalidOperationException()
    {
        Match match = StartClassicMatch();
        match.EndByAbandonment(FirstAccountId, _endedAt);

        Assert.Throws<InvalidOperationException>(() => match.EndByAbandonment(SecondAccountId, _endedAt));
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

    private static Match StartClassicMatch()
    {
        return Match.StartRanked(CreateClassicFormat(), _entrants, _startedAt);
    }

    private static MatchFormat CreateClassicFormat()
    {
        return CreateFormat(GameModeCode.Classic, ClassicBoardSize, ClassicWallsPerPlayer);
    }

    private static MatchFormat CreateFormat(GameModeCode code, byte boardSize, byte wallsPerPlayer)
    {
        var gameMode = new GameMode
        {
            Code = code,
            BoardSize = boardSize,
            PlayerCount = TwoPlayers,
            WallsPerPlayer = wallsPerPlayer,
            IsActive = true,
        };
        return new MatchFormat(gameMode, ClockMinutes);
    }

    private static Participation FindParticipation(Match match, int accountId)
    {
        return match.Participations.First(participation => participation.AccountId == accountId);
    }
}
