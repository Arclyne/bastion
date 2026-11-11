using System;
using System.Linq;
using Bastion.Contracts.Notifications;
using Bastion.Contracts.Rooms;

namespace Bastion.Client.Networking.Rooms;

public static class RoomContractTranslator
{
    public static RoomOptions ToContract(PrivateRoomOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return new RoomOptions
        {
            GameMode = ToContract(options.GameMode),
            WallsPerPlayer = options.WallsPerPlayer,
            ClockMinutes = options.ClockMinutes,
        };
    }

    public static PrivateRoom ToPrivateRoom(RoomState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        RoomOptions options = state.Options ?? new RoomOptions();
        RoomMemberState[] members = state.Members ?? Array.Empty<RoomMemberState>();
        return new PrivateRoom(
            state.Code ?? string.Empty,
            new PrivateRoomOptions(ToGameMode(options.GameMode), options.WallsPerPlayer, options.ClockMinutes),
            state.SeatCount,
            members.Select(ToMember).OrderBy(member => member.SeatIndex).ToList());
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
        switch (reason)
        {
            case RoomRejectionReason.NotificationsUnavailable:
                return RoomRejection.NotificationsUnavailable;
            case RoomRejectionReason.UnfinishedMatch:
                return RoomRejection.UnfinishedMatch;
            case RoomRejectionReason.SearchingMatch:
                return RoomRejection.SearchingMatch;
            case RoomRejectionReason.InRoom:
                return RoomRejection.InRoom;
            case RoomRejectionReason.InvalidOptions:
                return RoomRejection.InvalidSettings;
            case RoomRejectionReason.RoomNotFound:
                return RoomRejection.RoomNotFound;
            default:
                return ToPlacementRejection(reason);
        }
    }

    private static RoomRejection ToPlacementRejection(RoomRejectionReason reason)
    {
        switch (reason)
        {
            case RoomRejectionReason.RoomFull:
                return RoomRejection.RoomFull;
            case RoomRejectionReason.Kicked:
                return RoomRejection.Kicked;
            case RoomRejectionReason.TooManyAttempts:
                return RoomRejection.TooManyAttempts;
            case RoomRejectionReason.TooManyPlayers:
                return RoomRejection.TooManyPlayers;
            case RoomRejectionReason.NotInRoom:
                return RoomRejection.NotInRoom;
            case RoomRejectionReason.NotHost:
                return RoomRejection.NotHost;
            case RoomRejectionReason.NotReadyToStart:
                return RoomRejection.NotReadyToStart;
            case RoomRejectionReason.MatchStarted:
                return RoomRejection.MatchStarted;
            default:
                throw new ArgumentOutOfRangeException(nameof(reason), reason, "Unsupported room rejection reason.");
        }
    }

    private static PrivateRoomMember ToMember(RoomMemberState member)
    {
        return new PrivateRoomMember(member.Nickname ?? string.Empty, member.SeatIndex, member.IsReady, member.IsHost);
    }

    private static RoomGameMode ToContract(PrivateRoomGameMode gameMode)
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

    private static PrivateRoomGameMode ToGameMode(RoomGameMode gameMode)
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
}
