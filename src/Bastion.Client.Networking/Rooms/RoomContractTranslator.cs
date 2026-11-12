using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Contracts.Notifications;
using Bastion.Contracts.Rooms;

namespace Bastion.Client.Networking.Rooms;

public static class RoomContractTranslator
{
    private static readonly IReadOnlyDictionary<RoomRejectionReason, RoomRejection> _rejectionsByReason =
        new Dictionary<RoomRejectionReason, RoomRejection>
        {
            [RoomRejectionReason.NotificationsUnavailable] = RoomRejection.NotificationsUnavailable,
            [RoomRejectionReason.UnfinishedMatch] = RoomRejection.UnfinishedMatch,
            [RoomRejectionReason.SearchingMatch] = RoomRejection.SearchingMatch,
            [RoomRejectionReason.InRoom] = RoomRejection.InRoom,
            [RoomRejectionReason.InvalidOptions] = RoomRejection.InvalidOptions,
            [RoomRejectionReason.RoomNotFound] = RoomRejection.RoomNotFound,
            [RoomRejectionReason.RoomFull] = RoomRejection.RoomFull,
            [RoomRejectionReason.Kicked] = RoomRejection.Kicked,
            [RoomRejectionReason.TooManyAttempts] = RoomRejection.TooManyAttempts,
            [RoomRejectionReason.TooManyPlayers] = RoomRejection.TooManyPlayers,
            [RoomRejectionReason.NotInRoom] = RoomRejection.NotInRoom,
            [RoomRejectionReason.NotHost] = RoomRejection.NotHost,
            [RoomRejectionReason.NotReadyToStart] = RoomRejection.NotReadyToStart,
            [RoomRejectionReason.MatchStarted] = RoomRejection.MatchStarted,
        };

    public static RoomOptions ToRoomOptions(PrivateRoomOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        RoomGameMode gameMode = ToRoomGameMode(options.GameMode);
        return new RoomOptions
        {
            GameMode = gameMode,
            WallsPerPlayer = options.WallsPerPlayer,
            ClockMinutes = options.ClockMinutes,
        };
    }

    public static PrivateRoom ToPrivateRoom(RoomState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        string code = state.Code ?? string.Empty;
        RoomOptions options = state.Options ?? new RoomOptions();
        PrivateRoomGameMode gameMode = ToPrivateRoomGameMode(options.GameMode);
        var privateOptions = new PrivateRoomOptions(gameMode, options.WallsPerPlayer, options.ClockMinutes);
        RoomMemberState[] members = state.Members ?? Array.Empty<RoomMemberState>();
        IEnumerable<PrivateRoomMember> seatedMembers = members
            .Select(ToMember)
            .OrderBy(member => member.SeatIndex);
        var memberList = new List<PrivateRoomMember>(seatedMembers);
        return new PrivateRoom
        {
            Code = code,
            Options = privateOptions,
            SeatCount = state.SeatCount,
            Members = memberList,
        };
    }

    public static RoomCloseReason ToCloseReason(RoomClosedReason reason)
    {
        switch (reason)
        {
            case RoomClosedReason.HostLeft:
                return RoomCloseReason.HostLeft;
            default:
                throw new ArgumentOutOfRangeException(nameof(reason), reason, "Unsupported room closed reason.");
        }
    }

    public static RoomRejection ToRejection(RoomRejectionReason reason)
    {
        if (!_rejectionsByReason.TryGetValue(reason, out RoomRejection rejection))
        {
            throw new ArgumentOutOfRangeException(nameof(reason), reason, "Unsupported room rejection reason.");
        }

        return rejection;
    }

    private static RoomGameMode ToRoomGameMode(PrivateRoomGameMode gameMode)
    {
        switch (gameMode)
        {
            case PrivateRoomGameMode.Classic:
                return RoomGameMode.Classic;
            case PrivateRoomGameMode.FourPlayers:
                return RoomGameMode.FourPlayers;
            case PrivateRoomGameMode.Quick:
                return RoomGameMode.Quick;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(gameMode),
                    gameMode,
                    "Unsupported private room game mode.");
        }
    }

    private static PrivateRoomGameMode ToPrivateRoomGameMode(RoomGameMode gameMode)
    {
        switch (gameMode)
        {
            case RoomGameMode.Classic:
                return PrivateRoomGameMode.Classic;
            case RoomGameMode.FourPlayers:
                return PrivateRoomGameMode.FourPlayers;
            case RoomGameMode.Quick:
                return PrivateRoomGameMode.Quick;
            default:
                throw new ArgumentOutOfRangeException(nameof(gameMode), gameMode, "Unsupported room game mode.");
        }
    }

    private static PrivateRoomMember ToMember(RoomMemberState member)
    {
        string nickname = member.Nickname ?? string.Empty;
        return new PrivateRoomMember
        {
            Nickname = nickname,
            SeatIndex = member.SeatIndex,
            IsReady = member.IsReady,
            IsHost = member.IsHost,
        };
    }
}
