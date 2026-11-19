using Bastion.Client.Networking.Rooms;

namespace Bastion.Client.Tests.Screens.MainMenu.Rooms;

public static class PrivateRoomSampleFactory
{
    public const string Code = "K7MTQ3";
    public const string HostNickname = "marta_muros";
    public const string GuestNickname = "pedro_peon";
    public const byte ClassicWalls = 10;
    public const byte FewWalls = 6;
    public const byte FiveMinutes = 5;
    public const byte ThreeMinutes = 3;
    public const int TwoSeats = 2;
    public const int FourSeats = 4;

    private const int HostSeat = 0;
    private const int GuestSeat = 1;

    public static PrivateRoomOptions ClassicOptions { get; } =
        new PrivateRoomOptions(PrivateRoomGameMode.Classic, ClassicWalls, FiveMinutes);

    public static PrivateRoom CreateHostAlone()
    {
        return CreateRoom(ClassicOptions, [CreateHost()]);
    }

    public static PrivateRoom CreateWithGuest(bool isGuestReady)
    {
        return CreateRoom(ClassicOptions, [CreateHost(), CreateGuest(isGuestReady)]);
    }

    public static PrivateRoom CreateWithGuestAndOptions(PrivateRoomOptions options)
    {
        return CreateRoom(options, [CreateHost(), CreateGuest(isReady: false)]);
    }

    private static PrivateRoom CreateRoom(PrivateRoomOptions options, PrivateRoomMember[] members)
    {
        int seatCount = options.GameMode == PrivateRoomGameMode.FourPlayers ? FourSeats : TwoSeats;
        return new PrivateRoom
        {
            Code = Code,
            Options = options,
            SeatCount = seatCount,
            Members = members,
        };
    }

    private static PrivateRoomMember CreateHost()
    {
        return new PrivateRoomMember
        {
            Nickname = HostNickname,
            SeatIndex = HostSeat,
            IsReady = true,
            IsHost = true,
        };
    }

    private static PrivateRoomMember CreateGuest(bool isReady)
    {
        return new PrivateRoomMember
        {
            Nickname = GuestNickname,
            SeatIndex = GuestSeat,
            IsReady = isReady,
            IsHost = false,
        };
    }
}
