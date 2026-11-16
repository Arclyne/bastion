using System;
using System.Linq;
using Bastion.Domain.Rooms;
using Xunit;

namespace Bastion.Domain.Tests.Rooms;

public sealed class TestWaitingRoom
{
    private const int GuestAccountId = 8;
    private const string GuestNickname = "pedro_peon";
    private const int ThirdAccountId = 9;
    private const string ThirdNickname = "luis_quo";
    private const int FourthAccountId = 10;
    private const string FourthNickname = "sofia_salto";
    private const int FirstSeatIndex = 0;
    private const int SecondSeatIndex = 1;
    private const int RoomId = 3;
    private const string HostNickname = "marta_muros";

    [Fact]
    public void Constructor_Host_SitsReadyInFirstSeat()
    {
        (int SeatIndex, bool IsReady) expectedSeat = (FirstSeatIndex, true);
        var identity = new RoomIdentity(RoomId, RoomSampleFactory.Code);
        RoomSettings settings = RoomSampleFactory.CreateTwoPlayerSettings();
        var host = new RoomMember(RoomSampleFactory.HostAccountId, HostNickname);

        _ = new WaitingRoom(identity, settings, host);

        Assert.Equal(expectedSeat, (host.SeatIndex, host.IsReady));
    }

    [Fact]
    public void Admit_Guest_TakesNextSeatNotReady()
    {
        WaitingRoom room = RoomSampleFactory.CreateTwoPlayerRoom();
        RoomMember guest = CreateGuest();
        (int SeatIndex, bool IsReady) expectedSeat = (SecondSeatIndex, false);

        room.Admit(guest);

        Assert.Equal(expectedSeat, (guest.SeatIndex, guest.IsReady));
    }

    [Fact]
    public void Admit_FullRoom_ThrowsInvalidOperationException()
    {
        WaitingRoom room = RoomSampleFactory.CreateTwoPlayerRoom();
        AdmitGuest(room);
        RoomMember third = CreateThird();

        Action admitThird = () => room.Admit(third);

        Assert.Throws<InvalidOperationException>(admitThird);
    }

    [Fact]
    public void Admit_AfterMemberLeft_TakesFreedSeat()
    {
        WaitingRoom room = CreateFourPlayerRoomWithThreeMembers();
        room.Remove(GuestAccountId);
        var fourth = new RoomMember(FourthAccountId, FourthNickname);

        room.Admit(fourth);

        Assert.Equal(SecondSeatIndex, fourth.SeatIndex);
    }

    [Fact]
    public void IsReadyToStart_AllSeatsTakenAndReady_ReturnsTrue()
    {
        WaitingRoom room = CreateTwoPlayerRoomWithReadyGuest();

        bool isReadyToStart = room.IsReadyToStart();

        Assert.True(isReadyToStart);
    }

    [Fact]
    public void IsReadyToStart_MemberNotReady_ReturnsFalse()
    {
        WaitingRoom room = RoomSampleFactory.CreateTwoPlayerRoom();
        AdmitGuest(room);

        bool isReadyToStart = room.IsReadyToStart();

        Assert.False(isReadyToStart);
    }

    [Fact]
    public void IsReadyToStart_FreeSeat_ReturnsFalse()
    {
        WaitingRoom room = RoomSampleFactory.CreateFourPlayerRoom();
        AdmitGuest(room);
        room.SetReady(GuestAccountId, isReady: true);

        bool isReadyToStart = room.IsReadyToStart();

        Assert.False(isReadyToStart);
    }

    [Fact]
    public void SetReady_HostNotReady_KeepsHostReady()
    {
        WaitingRoom room = RoomSampleFactory.CreateTwoPlayerRoom();

        room.SetReady(RoomSampleFactory.HostAccountId, isReady: false);

        RoomMember host = room.Members.Single();
        Assert.True(host.IsReady);
    }

    [Fact]
    public void ChangeSettings_ReadyGuest_ClearsGuestReady()
    {
        WaitingRoom room = CreateTwoPlayerRoomWithReadyGuest();
        RoomSettings settings = RoomSampleFactory.CreateTwoPlayerSettings();

        room.ChangeSettings(settings);

        RoomMember guest = room.Members[SecondSeatIndex];
        Assert.False(guest.IsReady);
    }

    [Fact]
    public void ChangeSettings_FewerSeatsThanMembers_ThrowsInvalidOperationException()
    {
        WaitingRoom room = CreateFourPlayerRoomWithThreeMembers();
        RoomSettings settings = RoomSampleFactory.CreateTwoPlayerSettings();

        Action changeSettings = () => room.ChangeSettings(settings);

        Assert.Throws<InvalidOperationException>(changeSettings);
    }

    [Fact]
    public void ChangeSettings_FourToTwoSeats_SeatsMembersTogether()
    {
        WaitingRoom room = CreateFourPlayerRoomWithThreeMembers();
        room.Remove(GuestAccountId);
        RoomSettings settings = RoomSampleFactory.CreateTwoPlayerSettings();

        room.ChangeSettings(settings);

        RoomMember secondMember = room.Members[SecondSeatIndex];
        Assert.Equal(SecondSeatIndex, secondMember.SeatIndex);
    }

    [Fact]
    public void Remove_Host_ThrowsInvalidOperationException()
    {
        WaitingRoom room = RoomSampleFactory.CreateTwoPlayerRoom();

        Action removeHost = () => room.Remove(RoomSampleFactory.HostAccountId);

        Assert.Throws<InvalidOperationException>(removeHost);
    }

    private static WaitingRoom CreateFourPlayerRoomWithThreeMembers()
    {
        WaitingRoom room = RoomSampleFactory.CreateFourPlayerRoom();
        AdmitGuest(room);
        AdmitThird(room);
        return room;
    }

    private static void AdmitGuest(WaitingRoom room)
    {
        RoomMember guest = CreateGuest();
        room.Admit(guest);
    }

    private static RoomMember CreateGuest()
    {
        return new RoomMember(GuestAccountId, GuestNickname);
    }

    private static void AdmitThird(WaitingRoom room)
    {
        RoomMember third = CreateThird();
        room.Admit(third);
    }

    private static RoomMember CreateThird()
    {
        return new RoomMember(ThirdAccountId, ThirdNickname);
    }

    private static WaitingRoom CreateTwoPlayerRoomWithReadyGuest()
    {
        WaitingRoom room = RoomSampleFactory.CreateTwoPlayerRoom();
        AdmitGuest(room);
        room.SetReady(GuestAccountId, isReady: true);
        return room;
    }
}
