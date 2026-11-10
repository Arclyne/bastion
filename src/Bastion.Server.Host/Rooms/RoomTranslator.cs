using System;
using System.Linq;
using Bastion.Contracts.Notifications;
using Bastion.Contracts.Rooms;
using Bastion.Domain.GameModes;
using Bastion.Server.Application.Rooms;
using CoreWCF;

namespace Bastion.Server.Host.Rooms;

public static class RoomTranslator
{
    public static FaultException<RoomRejectedFault> ToFault(RoomRejectedException rejection)
    {
        ArgumentNullException.ThrowIfNull(rejection);

        var fault = new RoomRejectedFault
        {
            Reason = ToReason(rejection.Rejection),
            RetryAfterSeconds = (int)Math.Ceiling(rejection.RetryDelay.TotalSeconds),
        };
        return new FaultException<RoomRejectedFault>(fault, new FaultReason(rejection.Message));
    }

    public static RequestedRoomSettings ToRequestedSettings(RoomOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return new RequestedRoomSettings(
            ToGameModeCode(options.GameMode),
            options.WallsPerPlayer,
            options.ClockMinutes);
    }

    public static RoomState ToState(RoomSnapshot room)
    {
        ArgumentNullException.ThrowIfNull(room);

        return new RoomState
        {
            Code = room.Identity.Code,
            Options = ToOptions(room),
            SeatCount = room.GetSeatCount(),
            Members = room.Members.Select(ToMemberState).ToArray(),
        };
    }

    public static RoomUpdatedNotification ToUpdatedNotification(RoomSnapshot room, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(room);

        return new RoomUpdatedNotification
        {
            OccurredAt = now,
            Room = ToState(room),
        };
    }

    public static RoomClosedNotification ToClosedNotification(RoomSnapshot room, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(room);

        return new RoomClosedNotification
        {
            OccurredAt = now,
            Code = room.Identity.Code,
            Reason = RoomClosedReason.HostLeft,
        };
    }

    private static RoomOptions ToOptions(RoomSnapshot room)
    {
        return new RoomOptions
        {
            GameMode = ToRoomGameMode(room.Settings.GameMode.Code),
            WallsPerPlayer = room.Settings.WallsPerPlayer,
            ClockMinutes = room.Settings.ClockMinutes,
        };
    }

    private static RoomMemberState ToMemberState(RoomMemberSnapshot member)
    {
        return new RoomMemberState
        {
            Nickname = member.Nickname,
            SeatIndex = member.SeatIndex,
            IsReady = member.IsReady,
            IsHost = member.IsHost,
        };
    }

    private static GameModeCode ToGameModeCode(RoomGameMode gameMode)
    {
        switch (gameMode)
        {
            case RoomGameMode.Classic:
                return GameModeCode.Classic;
            case RoomGameMode.FourPlayers:
                return GameModeCode.FourPlayers;
            case RoomGameMode.Quick:
                return GameModeCode.Quick;
            default:
                throw new ArgumentOutOfRangeException(nameof(gameMode), gameMode, "Unsupported room game mode.");
        }
    }

    private static RoomGameMode ToRoomGameMode(GameModeCode code)
    {
        switch (code)
        {
            case GameModeCode.Classic:
                return RoomGameMode.Classic;
            case GameModeCode.FourPlayers:
                return RoomGameMode.FourPlayers;
            case GameModeCode.Quick:
                return RoomGameMode.Quick;
            default:
                throw new ArgumentOutOfRangeException(nameof(code), code, "Unsupported game mode code.");
        }
    }

    private static RoomRejectionReason ToReason(RoomRejection rejection)
    {
        switch (rejection)
        {
            case RoomRejection.NotificationsUnavailable:
                return RoomRejectionReason.NotificationsUnavailable;
            case RoomRejection.UnfinishedMatch:
                return RoomRejectionReason.UnfinishedMatch;
            case RoomRejection.SearchingMatch:
                return RoomRejectionReason.SearchingMatch;
            case RoomRejection.InRoom:
                return RoomRejectionReason.InRoom;
            case RoomRejection.InvalidSettings:
                return RoomRejectionReason.InvalidSettings;
            case RoomRejection.RoomNotFound:
                return RoomRejectionReason.RoomNotFound;
            default:
                return ToPlacementReason(rejection);
        }
    }

    private static RoomRejectionReason ToPlacementReason(RoomRejection rejection)
    {
        switch (rejection)
        {
            case RoomRejection.RoomFull:
                return RoomRejectionReason.RoomFull;
            case RoomRejection.Kicked:
                return RoomRejectionReason.Kicked;
            case RoomRejection.TooManyAttempts:
                return RoomRejectionReason.TooManyAttempts;
            case RoomRejection.TooManyPlayers:
                return RoomRejectionReason.TooManyPlayers;
            case RoomRejection.NotInRoom:
                return RoomRejectionReason.NotInRoom;
            case RoomRejection.NotHost:
                return RoomRejectionReason.NotHost;
            case RoomRejection.NotReadyToStart:
                return RoomRejectionReason.NotReadyToStart;
            case RoomRejection.MatchStarted:
                return RoomRejectionReason.MatchStarted;
            default:
                throw new ArgumentOutOfRangeException(nameof(rejection), rejection, "Unsupported room rejection.");
        }
    }
}
