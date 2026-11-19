using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Contracts.Matches;
using Bastion.Domain.Common;
using Bastion.Domain.Matches;
using Bastion.Server.Application.Matches;
using ContractMatchSnapshot = Bastion.Contracts.Matches.MatchSnapshot;
using ContractWallOrientation = Bastion.Contracts.Matches.WallOrientation;
using DomainMatchSnapshot = Bastion.Server.Application.Matches.MatchSnapshot;

namespace Bastion.Server.Host.Matches;

public static class MatchSnapshotTranslator
{
    private const byte NoTurnOrder = 0;

    public static OngoingMatch ToOngoingMatch(Lookup<DomainMatchSnapshot> foundSnapshot)
    {
        ArgumentNullException.ThrowIfNull(foundSnapshot);

        if (!foundSnapshot.IsFound)
        {
            return new OngoingMatch();
        }

        ContractMatchSnapshot snapshot = ToContractSnapshot(foundSnapshot.GetValue());
        return new OngoingMatch
        {
            IsFound = true,
            Snapshot = snapshot,
        };
    }

    public static ContractMatchSnapshot ToContractSnapshot(DomainMatchSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        Match match = snapshot.Match;
        IEnumerable<SnapshotPlayer> players = match.Participations
            .OrderBy(participation => participation.TurnOrder)
            .Select(participation => ToSnapshotPlayer(participation, snapshot));
        IEnumerable<SnapshotMove> moves = match.Moves
            .OrderBy(playedMove => playedMove.MoveNumber)
            .Select(playedMove => ToSnapshotMove(playedMove, match));
        byte wallsPerPlayer = CountWallsPerPlayer(match);
        int turnElapsedMilliseconds = MeasureTurnElapsed(match, snapshot.TakenAt);
        short nextMoveNumber = match.GetNextMoveNumber();
        byte pendingDrawOffererTurnOrder = FindPendingDrawOffererTurnOrder(match);
        int drawMovesLeft = CountDrawMovesLeft(match, snapshot.RequestingAccountId);
        var contractSnapshot = new ContractMatchSnapshot
        {
            MatchId = match.MatchId,
            BoardSize = snapshot.BoardSize,
            WallsPerPlayer = wallsPerPlayer,
            ClockMinutes = match.ClockMinutes,
            Players = [.. players],
            Moves = [.. moves],
            CurrentTurnOrder = match.CurrentTurnOrder,
            NextMoveNumber = nextMoveNumber,
            TurnElapsedMilliseconds = turnElapsedMilliseconds,
            PendingDrawOffererTurnOrder = pendingDrawOffererTurnOrder,
            DrawMovesLeft = drawMovesLeft,
        };
        AddEnding(contractSnapshot, snapshot);
        return contractSnapshot;
    }

    private static SnapshotPlayer ToSnapshotPlayer(Participation participation, DomainMatchSnapshot snapshot)
    {
        string nickname = snapshot.NicknamesByAccountId.GetValueOrDefault(participation.AccountId, string.Empty);
        int disconnectedMilliseconds = MeasureTimeAway(participation, snapshot.TakenAt);
        return new SnapshotPlayer
        {
            TurnOrder = participation.TurnOrder,
            Nickname = nickname,
            EloRating = participation.InitialElo,
            Square = participation.CurrentSquare,
            RemainingWalls = participation.RemainingWalls,
            RemainingClockMilliseconds = participation.RemainingClockMilliseconds,
            HasEnded = participation.HasEnded(),
            IsConnected = participation.IsConnected,
            DisconnectedMilliseconds = disconnectedMilliseconds,
        };
    }

    private static int MeasureTimeAway(Participation participation, DateTime takenAt)
    {
        if (participation.IsConnected || participation.DisconnectedAt is not DateTime disconnectedAt)
        {
            return 0;
        }

        TimeSpan timeAway = takenAt - disconnectedAt;
        double awayMilliseconds = Math.Max(timeAway.TotalMilliseconds, 0);
        int wholeMilliseconds = (int)awayMilliseconds;
        return wholeMilliseconds;
    }

    private static SnapshotMove ToSnapshotMove(Move playedMove, Match match)
    {
        Participation mover = match.GetParticipation(playedMove.AccountId);
        bool isWall = playedMove.MoveType == MoveType.Wall;
        ContractWallOrientation orientation = isWall
            ? MatchTranslator.ToContractOrientation(playedMove.GetWall().Orientation)
            : ContractWallOrientation.Horizontal;
        return new SnapshotMove
        {
            MoveNumber = playedMove.MoveNumber,
            TurnOrder = mover.TurnOrder,
            IsWall = isWall,
            Groove = playedMove.Groove,
            Orientation = orientation,
        };
    }

    private static byte CountWallsPerPlayer(Match match)
    {
        int mostWalls = 0;
        foreach (Participation participation in match.Participations)
        {
            int placedWalls = match.Moves.Count(
                playedMove => playedMove.AccountId == participation.AccountId && playedMove.MoveType == MoveType.Wall);
            mostWalls = Math.Max(mostWalls, participation.RemainingWalls + placedWalls);
        }

        byte wallsPerPlayer = (byte)mostWalls;
        return wallsPerPlayer;
    }

    private static int MeasureTurnElapsed(Match match, DateTime takenAt)
    {
        if (match.IsFinished())
        {
            return 0;
        }

        TimeSpan turnTime = takenAt - match.TurnStartedAt;
        double elapsedMilliseconds = Math.Max(turnTime.TotalMilliseconds, 0);
        int wholeMilliseconds = (int)elapsedMilliseconds;
        return wholeMilliseconds;
    }

    private static byte FindPendingDrawOffererTurnOrder(Match match)
    {
        DrawOffer? pendingOffer = match.DrawOffers.FirstOrDefault(drawOffer => drawOffer.IsPending());
        if (pendingOffer is null)
        {
            return NoTurnOrder;
        }

        Participation offerer = match.GetParticipation(pendingOffer.OffererId);
        return offerer.TurnOrder;
    }

    private static int CountDrawMovesLeft(Match match, int requestingAccountId)
    {
        if (!match.IsPlayedBy(requestingAccountId))
        {
            return 0;
        }

        return match.CountMovesBeforeNextDrawOffer(requestingAccountId);
    }

    private static void AddEnding(ContractMatchSnapshot contractSnapshot, DomainMatchSnapshot snapshot)
    {
        int matchId = snapshot.Match.MatchId;
        if (snapshot.Ending is not null)
        {
            contractSnapshot.Ending = MatchTranslator.ToMatchEndedNotification(matchId, snapshot.Ending, snapshot.TakenAt);
        }

        if (snapshot.OwnDeparture is not null)
        {
            contractSnapshot.OwnDeparture = MatchTranslator.ToParticipationEndedNotification(
                matchId,
                snapshot.OwnDeparture,
                snapshot.TakenAt);
        }
    }
}
