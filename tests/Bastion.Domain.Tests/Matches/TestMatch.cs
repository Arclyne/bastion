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
    private const byte RoomWallsPerPlayer = 3;
    private const int ThirdAccountId = 7;
    private const int FourthAccountId = 8;
    private const byte FourPlayers = 4;
    private const byte FourPlayerWallsPerPlayer = 5;
    private const byte LastOfFourPlacement = 4;
    private const byte ThirdPlacement = 3;
    private const byte FirstPlacement = 1;

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
    public void StartPrivate_TwoEntrants_CreatesPrivateMatch()
    {
        MatchFormat format = CreateClassicFormat();

        var match = Match.StartPrivate(format, CreatePrivateEntrants(), _startedAt);

        Assert.Equal(MatchType.Private, match.MatchType);
    }

    [Fact]
    public void StartPrivate_WallsChosenForRoom_GivesThoseWallsToEveryPlayer()
    {
        MatchFormat format = CreateClassicFormat() with { WallsPerPlayer = RoomWallsPerPlayer };

        var match = Match.StartPrivate(format, CreatePrivateEntrants(), _startedAt);

        Assert.True(match.Participations.All(participation => participation.RemainingWalls == RoomWallsPerPlayer));
    }

    [Fact]
    public void StartPrivate_EntrantsWithoutElo_SavesNoInitialElo()
    {
        MatchFormat format = CreateClassicFormat();

        var match = Match.StartPrivate(format, CreatePrivateEntrants(), _startedAt);

        Assert.True(match.Participations.All(participation => participation.InitialElo is null));
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
    public void Abandon_FirstPlayerLeaves_FirstPlayerLoses()
    {
        Match match = StartClassicMatch();

        match.Abandon(FirstAccountId, _endedAt);

        Assert.Equal(ParticipationResult.Lost, FindParticipation(match, FirstAccountId).Result);
    }

    [Fact]
    public void Abandon_FirstPlayerLeaves_RivalWins()
    {
        Match match = StartClassicMatch();

        match.Abandon(FirstAccountId, _endedAt);

        Assert.Equal(ParticipationResult.Won, FindParticipation(match, SecondAccountId).Result);
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

        Assert.Throws<InvalidOperationException>(() => match.Abandon(SecondAccountId, _endedAt));
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

    [Fact]
    public void Abandon_FourPlayersFirstLeaves_KeepsMatchGoing()
    {
        Match match = StartFourPlayerMatch();

        bool isMatchOver = match.Abandon(FirstAccountId, _endedAt);

        Assert.False(isMatchOver);
    }

    [Fact]
    public void Abandon_FourPlayersFirstLeaves_GivesLeaverLastPlace()
    {
        Match match = StartFourPlayerMatch();

        match.Abandon(FirstAccountId, _endedAt);

        Participation leaver = FindParticipation(match, FirstAccountId);
        Assert.Equal(
            (ParticipationResult.Lost, MatchEndReason.Abandonment, LastOfFourPlacement),
            (leaver.Result, leaver.EndReason, leaver.Placement));
    }

    [Fact]
    public void Abandon_FourPlayersSecondLeaver_GivesThirdPlace()
    {
        Match match = StartFourPlayerMatch();
        match.Abandon(FirstAccountId, _endedAt);

        match.Abandon(ThirdAccountId, _endedAt);

        Assert.Equal(ThirdPlacement, FindParticipation(match, ThirdAccountId).Placement);
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

        match.Abandon(ThirdAccountId, _endedAt);

        Participation winner = FindParticipation(match, FourthAccountId);
        Assert.Equal(
            (ParticipationResult.Won, FirstPlacement, MatchStatus.Finished),
            (winner.Result, winner.Placement, match.Status));
    }

    private static Match StartFourPlayerMatch()
    {
        var gameMode = new GameMode
        {
            Code = GameModeCode.FourPlayers,
            BoardSize = ClassicBoardSize,
            PlayerCount = FourPlayers,
            WallsPerPlayer = FourPlayerWallsPerPlayer,
            IsActive = true,
        };
        var entrants = new MatchEntrant[]
        {
            new MatchEntrant(FirstAccountId, null),
            new MatchEntrant(SecondAccountId, null),
            new MatchEntrant(ThirdAccountId, null),
            new MatchEntrant(FourthAccountId, null),
        };
        return Match.StartPrivate(new MatchFormat(gameMode, ClockMinutes), entrants, _startedAt);
    }

    private static MatchEntrant[] CreatePrivateEntrants()
    {
        return new MatchEntrant[]
        {
            new MatchEntrant(FirstAccountId, null),
            new MatchEntrant(SecondAccountId, null),
        };
    }

    private static Participation FindParticipation(Match match, int accountId)
    {
        return match.Participations.First(participation => participation.AccountId == accountId);
    }
}
