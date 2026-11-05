using Bastion.Domain.GameModes;
using Bastion.Domain.Rooms;

namespace Bastion.Domain.Tests.Rooms;

public static class RoomSamples
{
    public const int RoomId = 3;
    public const string Code = "K7MTQ3";
    public const int HostAccountId = 7;
    public const string HostNickname = "marta_muros";
    public const byte WallsPerPlayer = 10;
    public const byte ClockMinutes = 5;

    private const byte BoardSize = 9;
    private const byte TwoPlayers = 2;
    private const byte FourPlayers = 4;

    public static RoomSettings CreateTwoPlayerSettings()
    {
        return CreateSettings(TwoPlayers);
    }

    public static RoomSettings CreateFourPlayerSettings()
    {
        return CreateSettings(FourPlayers);
    }

    public static WaitingRoom CreateTwoPlayerRoom()
    {
        return CreateRoom(CreateTwoPlayerSettings());
    }

    public static WaitingRoom CreateFourPlayerRoom()
    {
        return CreateRoom(CreateFourPlayerSettings());
    }

    private static WaitingRoom CreateRoom(RoomSettings settings)
    {
        return new WaitingRoom(
            new RoomIdentity(RoomId, Code),
            settings,
            new RoomMember(HostAccountId, HostNickname));
    }

    private static RoomSettings CreateSettings(byte playerCount)
    {
        var gameMode = new GameMode
        {
            Code = playerCount == FourPlayers ? GameModeCode.FourPlayers : GameModeCode.Classic,
            BoardSize = BoardSize,
            PlayerCount = playerCount,
            WallsPerPlayer = WallsPerPlayer,
            IsActive = true,
        };
        return new RoomSettings(gameMode, WallsPerPlayer, ClockMinutes);
    }
}
