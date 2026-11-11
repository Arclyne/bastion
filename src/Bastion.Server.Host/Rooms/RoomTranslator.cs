using System;
using System.Collections.Generic;
using Bastion.Contracts.Notifications;
using Bastion.Contracts.Rooms;
using Bastion.Domain.GameModes;
using Bastion.Server.Application.Rooms;
using Bastion.Server.Host.Common;
using CoreWCF;

namespace Bastion.Server.Host.Rooms;

public static class RoomTranslator
{
    private static readonly IReadOnlyDictionary<RoomRejection, RoomRejectionReason> _reasonsByRejection =
        new Dictionary<RoomRejection, RoomRejectionReason>
        {
            [RoomRejection.NotificationsUnavailable] = RoomRejectionReason.NotificationsUnavailable,
            [RoomRejection.UnfinishedMatch] = RoomRejectionReason.UnfinishedMatch,
            [RoomRejection.SearchingMatch] = RoomRejectionReason.SearchingMatch,
            [RoomRejection.InRoom] = RoomRejectionReason.InRoom,
            [RoomRejection.InvalidSettings] = RoomRejectionReason.InvalidOptions,
            [RoomRejection.RoomNotFound] = RoomRejectionReason.RoomNotFound,
            [RoomRejection.RoomFull] = RoomRejectionReason.RoomFull,
            [RoomRejection.Kicked] = RoomRejectionReason.Kicked,
            [RoomRejection.TooManyAttempts] = RoomRejectionReason.TooManyAttempts,
            [RoomRejection.TooManyPlayers] = RoomRejectionReason.TooManyPlayers,
            [RoomRejection.NotInRoom] = RoomRejectionReason.NotInRoom,
            [RoomRejection.NotHost] = RoomRejectionReason.NotHost,
            [RoomRejection.NotReadyToStart] = RoomRejectionReason.NotReadyToStart,
            [RoomRejection.MatchStarted] = RoomRejectionReason.MatchStarted,
        };

    public static FaultException<RoomRejectedFault> ToFault(RoomRejectedException rejection)
    {
        ArgumentNullException.ThrowIfNull(rejection);

        double retryAfterSeconds = Math.Ceiling(rejection.RetryDelay.TotalSeconds);
        var fault = new RoomRejectedFault
        {
            Reason = ToReason(rejection.Rejection),
            RetryAfterSeconds = (int)retryAfterSeconds,
        };
        return FaultExceptionFactory.Create(fault, rejection);
    }

    public static RequestedRoomSettings ToRequestedSettings(RoomOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        GameModeCode gameModeCode = ToGameModeCode(options.GameMode);
        return new RequestedRoomSettings(gameModeCode, options.WallsPerPlayer, options.ClockMinutes);
    }

    public static RoomState ToState(RoomSnapshot room)
    {
        ArgumentNullException.ThrowIfNull(room);

        RoomOptions options = ToOptions(room);
        int seatCount = room.GetSeatCount();
        RoomMemberState[] members = ContractArray.ConvertAll(room.Members, ToMemberState);
        return new RoomState
        {
            Code = room.Identity.Code,
            Options = options,
            SeatCount = seatCount,
            Members = members,
        };
    }

    public static RoomUpdatedNotification ToUpdatedNotification(RoomSnapshot room, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(room);

        RoomState state = ToState(room);
        return new RoomUpdatedNotification
        {
            OccurredAt = now,
            Room = state,
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

    private static RoomRejectionReason ToReason(RoomRejection rejection)
    {
        if (!_reasonsByRejection.TryGetValue(rejection, out RoomRejectionReason reason))
        {
            throw new ArgumentOutOfRangeException(nameof(rejection), rejection, "Unsupported room rejection.");
        }

        return reason;
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

    private static RoomOptions ToOptions(RoomSnapshot room)
    {
        RoomGameMode gameMode = ToRoomGameMode(room.Settings.GameMode.Code);
        return new RoomOptions
        {
            GameMode = gameMode,
            WallsPerPlayer = room.Settings.WallsPerPlayer,
            ClockMinutes = room.Settings.ClockMinutes,
        };
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
}
