using Bastion.Client.Networking.Matches;
using Bastion.Client.Networking.Notifications;
using Bastion.Domain.Matches;

namespace Bastion.Client.Tests.Screens.Matches;

public static class MatchEndSampleFactory
{
    public const short OwnInitialElo = 1210;
    public const short OwnWinFinalElo = 1222;
    public const short OwnLossFinalElo = 1190;
    public const short RivalInitialElo = 1000;
    public const int MoveCount = 37;
    public const int PlayedSeconds = 768;
    public const byte OwnRemainingWalls = 3;
    public const int TwoActivePlayers = 2;
    public const byte SecondPlacement = 2;

    private const short RivalWinFinalElo = 1020;
    private const short RivalLossFinalElo = 988;
    private const byte RivalRemainingWalls = 4;
    private const short FourPlayerNextMoveNumber = 40;
    private const byte FirstPlacement = 1;
    private const byte LastPlacement = 4;

    public static MatchEndedEventArgs CreateOwnGoalWin(byte ownTurnOrder)
    {
        PlayerStanding own = CreateStanding(ownTurnOrder, StandingResult.Won, (OwnInitialElo, OwnWinFinalElo));
        byte rivalTurnOrder = FindRivalTurnOrder(ownTurnOrder);
        (short? Initial, short? Final) rivalElo = (RivalInitialElo, RivalLossFinalElo);
        PlayerStanding rival = CreateStanding(rivalTurnOrder, StandingResult.Lost, rivalElo);
        PlayerStanding[] standings = OrderByTurn(own, rival);
        return CreateEnd(EndedMatchReason.Goal, ownTurnOrder, standings);
    }

    public static MatchEndedEventArgs CreateRivalGoalWin(byte ownTurnOrder)
    {
        PlayerStanding own = CreateStanding(ownTurnOrder, StandingResult.Lost, (OwnInitialElo, OwnLossFinalElo));
        byte rivalTurnOrder = FindRivalTurnOrder(ownTurnOrder);
        PlayerStanding rival = CreateStanding(rivalTurnOrder, StandingResult.Won, (RivalInitialElo, RivalWinFinalElo));
        PlayerStanding[] standings = OrderByTurn(own, rival);
        return CreateEnd(EndedMatchReason.Goal, rivalTurnOrder, standings);
    }

    public static MatchEndedEventArgs CreateRivalAbandonment(byte ownTurnOrder)
    {
        PlayerStanding own = CreateStanding(ownTurnOrder, StandingResult.Won, (OwnInitialElo, OwnWinFinalElo));
        byte rivalTurnOrder = FindRivalTurnOrder(ownTurnOrder);
        (short? Initial, short? Final) rivalElo = (RivalInitialElo, RivalLossFinalElo);
        PlayerStanding rival = CreateStanding(rivalTurnOrder, StandingResult.Lost, rivalElo);
        PlayerStanding[] standings = OrderByTurn(own, rival);
        return CreateEnd(EndedMatchReason.Abandonment, rivalTurnOrder, standings);
    }

    public static MatchEndedEventArgs CreateOwnAbandonment(byte ownTurnOrder)
    {
        PlayerStanding own = CreateStanding(ownTurnOrder, StandingResult.Lost, (OwnInitialElo, OwnLossFinalElo));
        byte rivalTurnOrder = FindRivalTurnOrder(ownTurnOrder);
        PlayerStanding rival = CreateStanding(rivalTurnOrder, StandingResult.Won, (RivalInitialElo, RivalWinFinalElo));
        PlayerStanding[] standings = OrderByTurn(own, rival);
        return CreateEnd(EndedMatchReason.Abandonment, ownTurnOrder, standings);
    }

    public static MatchEndedEventArgs CreatePrivateOwnGoalWin(byte ownTurnOrder)
    {
        PlayerStanding own = CreateStanding(ownTurnOrder, StandingResult.Won, (null, null));
        byte rivalTurnOrder = FindRivalTurnOrder(ownTurnOrder);
        PlayerStanding rival = CreateStanding(rivalTurnOrder, StandingResult.Lost, (null, null));
        PlayerStanding[] standings = OrderByTurn(own, rival);
        return new MatchEndedEventArgs
        {
            MatchId = JoinedMatchSampleFactory.MatchId,
            EndReason = EndedMatchReason.Goal,
            DecidingTurnOrder = ownTurnOrder,
            IsRanked = false,
            MoveCount = MoveCount,
            PlayedSeconds = PlayedSeconds,
            Standings = standings,
        };
    }

    public static ParticipationEndedEventArgs CreateOwnThirdTurnSecondPlace()
    {
        PlayerStanding[] standings =
        [
            CreateDepartedStanding(Match.FirstTurnOrder, FirstPlacement, EndedMatchReason.Goal),
            CreateStanding(Match.SecondTurnOrder, StandingResult.StillPlaying, (null, null)),
            CreateDepartedStanding(Match.ThirdTurnOrder, SecondPlacement, EndedMatchReason.Goal),
            CreateStanding(Match.FourthTurnOrder, StandingResult.StillPlaying, (null, null)),
        ];
        return new ParticipationEndedEventArgs
        {
            MatchId = JoinedMatchSampleFactory.MatchId,
            EndReason = EndedMatchReason.Goal,
            DepartingTurnOrder = Match.ThirdTurnOrder,
            ActivePlayerCount = TwoActivePlayers,
            NextTurnOrder = Match.FourthTurnOrder,
            NextMoveNumber = FourPlayerNextMoveNumber,
            MoveCount = MoveCount,
            PlayedSeconds = PlayedSeconds,
            Standings = standings,
        };
    }

    public static ParticipationEndedEventArgs CreateDeparture(
        int matchId,
        byte departingTurnOrder,
        EndedMatchReason endReason)
    {
        PlayerStanding departing = CreateDepartedStanding(departingTurnOrder, LastPlacement, endReason);
        return new ParticipationEndedEventArgs
        {
            MatchId = matchId,
            EndReason = endReason,
            DepartingTurnOrder = departingTurnOrder,
            ActivePlayerCount = TwoActivePlayers,
            NextTurnOrder = Match.FirstTurnOrder,
            NextMoveNumber = FourPlayerNextMoveNumber,
            MoveCount = MoveCount,
            PlayedSeconds = PlayedSeconds,
            Standings = [departing],
        };
    }

    private static MatchEndedEventArgs CreateEnd(
        EndedMatchReason endReason,
        byte decidingTurnOrder,
        PlayerStanding[] standings)
    {
        return new MatchEndedEventArgs
        {
            MatchId = JoinedMatchSampleFactory.MatchId,
            EndReason = endReason,
            DecidingTurnOrder = decidingTurnOrder,
            IsRanked = true,
            MoveCount = MoveCount,
            PlayedSeconds = PlayedSeconds,
            Standings = standings,
        };
    }

    private static byte FindRivalTurnOrder(byte ownTurnOrder)
    {
        return ownTurnOrder == Match.FirstTurnOrder ? Match.SecondTurnOrder : Match.FirstTurnOrder;
    }

    private static PlayerStanding[] OrderByTurn(PlayerStanding own, PlayerStanding rival)
    {
        if (own.TurnOrder < rival.TurnOrder)
        {
            return [own, rival];
        }

        return [rival, own];
    }

    private static PlayerStanding CreateStanding(
        byte turnOrder,
        StandingResult result,
        (short? Initial, short? Final) elo)
    {
        return new PlayerStanding
        {
            TurnOrder = turnOrder,
            Result = result,
            Placement = null,
            EndReason = null,
            InitialElo = elo.Initial,
            FinalElo = elo.Final,
            RemainingWalls = result == StandingResult.Lost ? RivalRemainingWalls : OwnRemainingWalls,
        };
    }

    private static PlayerStanding CreateDepartedStanding(byte turnOrder, byte placement, EndedMatchReason endReason)
    {
        StandingResult result = placement == FirstPlacement ? StandingResult.Won : StandingResult.Lost;
        return new PlayerStanding
        {
            TurnOrder = turnOrder,
            Result = result,
            Placement = placement,
            EndReason = endReason,
            InitialElo = null,
            FinalElo = null,
            RemainingWalls = OwnRemainingWalls,
        };
    }
}
