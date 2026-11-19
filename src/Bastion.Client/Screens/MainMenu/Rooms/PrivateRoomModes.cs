using System;
using System.Collections.Generic;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Rooms;

namespace Bastion.Client.Screens.MainMenu.Rooms;

public static class PrivateRoomModes
{
    private const int TwoSeats = 2;
    private const int FourSeats = 4;
    private const int LargeBoardSize = 9;
    private const int SmallBoardSize = 7;
    private const byte ClassicWalls = 10;
    private const byte FourPlayersWalls = 5;
    private const byte QuickWalls = 6;

    public static IReadOnlyList<PrivateRoomGameMode> All { get; } =
    [
        PrivateRoomGameMode.Classic,
        PrivateRoomGameMode.FourPlayers,
        PrivateRoomGameMode.Quick,
    ];

    public static int GetSeatCount(PrivateRoomGameMode mode)
    {
        return mode == PrivateRoomGameMode.FourPlayers ? FourSeats : TwoSeats;
    }

    public static int GetBoardSize(PrivateRoomGameMode mode)
    {
        return mode == PrivateRoomGameMode.Quick ? SmallBoardSize : LargeBoardSize;
    }

    public static byte GetProposedWalls(PrivateRoomGameMode mode)
    {
        switch (mode)
        {
            case PrivateRoomGameMode.Classic:
                return ClassicWalls;
            case PrivateRoomGameMode.FourPlayers:
                return FourPlayersWalls;
            case PrivateRoomGameMode.Quick:
                return QuickWalls;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unsupported room game mode.");
        }
    }

    public static string GetName(PrivateRoomGameMode mode)
    {
        switch (mode)
        {
            case PrivateRoomGameMode.Classic:
                return Strings.RoomClassicName;
            case PrivateRoomGameMode.FourPlayers:
                return Strings.RoomFourPlayersName;
            case PrivateRoomGameMode.Quick:
                return Strings.RoomQuickName;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unsupported room game mode.");
        }
    }
}
