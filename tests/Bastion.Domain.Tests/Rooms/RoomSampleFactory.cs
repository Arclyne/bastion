using Bastion.Domain.GameModes;
using Bastion.Domain.Rooms;
using Bastion.Domain.Tests.GameModes;

namespace Bastion.Domain.Tests.Rooms;

public static class RoomSampleFactory
{
    public const string Code = "K7MTQ3";
    public const int HostAccountId = 7;

    private const int RoomId = 3;
    private const string HostNickname = "marta_muros";
    private const byte WallsPerPlayer = 10;
    private const byte ClockMinutes = 5;

    public static RoomSettings CreateTwoPlayerSettings()
    {
        GameMode gameMode = GameModeSampleFactory.CreateClassic();
        return CreateSettings(gameMode);
    }

    public static WaitingRoom CreateTwoPlayerRoom()
    {
        RoomSettings settings = CreateTwoPlayerSettings();
        return CreateRoom(settings);
    }

    public static WaitingRoom CreateFourPlayerRoom()
    {
        RoomSettings settings = CreateFourPlayerSettings();
        return CreateRoom(settings);
    }

    private static RoomSettings CreateSettings(GameMode gameMode)
    {
        return new RoomSettings(gameMode, WallsPerPlayer, ClockMinutes);
    }

    private static WaitingRoom CreateRoom(RoomSettings settings)
    {
        var identity = new RoomIdentity(RoomId, Code);
        var host = new RoomMember(HostAccountId, HostNickname);
        return new WaitingRoom(identity, settings, host);
    }

    private static RoomSettings CreateFourPlayerSettings()
    {
        GameMode gameMode = GameModeSampleFactory.CreateFourPlayers();
        return CreateSettings(gameMode);
    }
}
