using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Client.Networking.Matchmaking;
using Bastion.Client.Networking.Notifications;
using Bastion.Contracts.Matches;

namespace Bastion.Client.Networking.Matches;

public static class MatchSnapshotTranslator
{
    public static FoundOngoingMatch ToOngoingMatch(OngoingMatch ongoingMatch)
    {
        ArgumentNullException.ThrowIfNull(ongoingMatch);

        if (!ongoingMatch.IsFound || ongoingMatch.Snapshot is null)
        {
            return FoundOngoingMatch.None;
        }

        RejoinedMatch rejoinedMatch = ToRejoinedMatch(ongoingMatch.Snapshot);
        return new FoundOngoingMatch
        {
            Match = rejoinedMatch,
        };
    }

    public static RejoinedMatch ToRejoinedMatch(MatchSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        SnapshotPlayer[] snapshotPlayers = snapshot.Players ?? Array.Empty<SnapshotPlayer>();
        SnapshotMove[] snapshotMoves = snapshot.Moves ?? Array.Empty<SnapshotMove>();
        MatchFoundEventArgs foundMatch = ToFoundMatch(snapshot, snapshotPlayers);
        IEnumerable<RejoinedPlayer> players = snapshotPlayers.Select(ToRejoinedPlayer);
        IEnumerable<RejoinedMove> moves = snapshotMoves.Select(ToRejoinedMove);
        var rejoinedMatch = new RejoinedMatch
        {
            FoundMatch = foundMatch,
            Players = [.. players],
            Moves = [.. moves],
            CurrentTurnOrder = snapshot.CurrentTurnOrder,
            NextMoveNumber = snapshot.NextMoveNumber,
            TurnElapsed = TimeSpan.FromMilliseconds(snapshot.TurnElapsedMilliseconds),
            PendingDrawOffererTurnOrder = snapshot.PendingDrawOffererTurnOrder,
            DrawMovesLeft = snapshot.DrawMovesLeft,
        };
        if (snapshot.Ending is not null)
        {
            rejoinedMatch = rejoinedMatch with { Ending = MatchContractTranslator.ToMatchEndedEventArgs(snapshot.Ending) };
        }

        if (snapshot.OwnDeparture is not null)
        {
            var ownDeparture =
                MatchContractTranslator.ToParticipationEndedEventArgs(snapshot.OwnDeparture);
            rejoinedMatch = rejoinedMatch with { OwnDeparture = ownDeparture };
        }

        return rejoinedMatch;
    }

    private static MatchFoundEventArgs ToFoundMatch(MatchSnapshot snapshot, SnapshotPlayer[] snapshotPlayers)
    {
        var format = new FoundMatchFormat(snapshot.BoardSize, snapshot.WallsPerPlayer, snapshot.ClockMinutes);
        IEnumerable<FoundMatchParticipant> participants = snapshotPlayers.Select(ToFoundMatchParticipant);
        return new MatchFoundEventArgs(snapshot.MatchId, format, [.. participants]);
    }

    private static FoundMatchParticipant ToFoundMatchParticipant(SnapshotPlayer player)
    {
        string nickname = player.Nickname ?? string.Empty;
        return new FoundMatchParticipant(nickname, player.EloRating, player.TurnOrder);
    }

    private static RejoinedPlayer ToRejoinedPlayer(SnapshotPlayer player)
    {
        int remainingMilliseconds = player.RemainingClockMilliseconds.GetValueOrDefault();
        return new RejoinedPlayer
        {
            TurnOrder = player.TurnOrder,
            Square = player.Square ?? string.Empty,
            RemainingWalls = player.RemainingWalls,
            RemainingClock = TimeSpan.FromMilliseconds(remainingMilliseconds),
            HasEnded = player.HasEnded,
            IsConnected = player.IsConnected,
            TimeAway = TimeSpan.FromMilliseconds(player.DisconnectedMilliseconds),
        };
    }

    private static RejoinedMove ToRejoinedMove(SnapshotMove snapshotMove)
    {
        PlacedWallOrientation orientation = MatchContractTranslator.ToPlacedOrientation(snapshotMove.Orientation);
        return new RejoinedMove
        {
            MoveNumber = snapshotMove.MoveNumber,
            TurnOrder = snapshotMove.TurnOrder,
            IsWall = snapshotMove.IsWall,
            Groove = snapshotMove.Groove ?? string.Empty,
            Orientation = orientation,
        };
    }
}
