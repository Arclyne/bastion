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

    [Fact]
    public void Constructor_Host_SitsReadyInFirstSeat()
    {
        (int SeatIndex, bool IsReady) expectedSeat = (FirstSeatIndex, true);

        WaitingRoom room = RoomSampleFactory.CreateTwoPlayerRoom();

        RoomMember host = room.Members.Single();
        Assert.Equal(expectedSeat, (host.SeatIndex, host.IsReady));
    }

    [Fact]
    public void Admit_Guest_TakesNextSeatNotReady()
    {
        WaitingRoom room = RoomSampleFactory.CreateTwoPlayerRoom();
        var guest = new RoomMember(GuestAccountId, GuestNickname);
        (int SeatIndex, bool IsReady) expectedSeat = (SecondSeatIndex, false);

        room.Admit(guest);

        Assert.Equal(expectedSeat, (guest.SeatIndex, guest.IsReady));
    }

    [Fact]
    public void Admit_FullRoom_ThrowsInvalidOperation()
    {
        WaitingRoom room = RoomSampleFactory.CreateTwoPlayerRoom();
        AdmitGuest(room);
        var third = new RoomMember(ThirdAccountId, ThirdNickname);

        Action admitThird = () => room.Admit(third);

        Assert.Throws<InvalidOperationException>(admitThird);
    }

    [Fact]
    public void Admit_AfterMemberLeft_TakesFreedSeat()
    {
        WaitingRoom room = RoomSampleFactory.CreateFourPlayerRoom();
        AdmitGuest(room);
        AdmitThird(room);
        room.Remove(GuestAccountId);
        var fourth = new RoomMember(FourthAccountId, FourthNickname);

        room.Admit(fourth);

        Assert.Equal(SecondSeatIndex, fourth.SeatIndex);
    }

    [Fact]
    public void IsReadyToStart_AllSeatsTakenAndReady_ReturnsTrue()
    {
        WaitingRoom room = RoomSampleFactory.CreateTwoPlayerRoom();
        AdmitGuest(room);
        room.SetReady(GuestAccountId, isReady: true);

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
        WaitingRoom room = RoomSampleFactory.CreateTwoPlayerRoom();
        AdmitGuest(room);
        room.SetReady(GuestAccountId, isReady: true);
        RoomSettings settings = RoomSampleFactory.CreateTwoPlayerSettings();

        room.ChangeSettings(settings);

        RoomMember guest = room.Members[SecondSeatIndex];
        Assert.False(guest.IsReady);
    }

    [Fact]
    public void ChangeSettings_FewerSeatsThanMembers_ThrowsInvalidOperation()
    {
        WaitingRoom room = RoomSampleFactory.CreateFourPlayerRoom();
        AdmitGuest(room);
        AdmitThird(room);
        RoomSettings settings = RoomSampleFactory.CreateTwoPlayerSettings();

        Action changeSettings = () => room.ChangeSettings(settings);

        Assert.Throws<InvalidOperationException>(changeSettings);
    }

    [Fact]
    public void ChangeSettings_FourToTwoSeats_SeatsMembersTogether()
    {
        WaitingRoom room = RoomSampleFactory.CreateFourPlayerRoom();
        AdmitGuest(room);
        AdmitThird(room);
        room.Remove(GuestAccountId);
        RoomSettings settings = RoomSampleFactory.CreateTwoPlayerSettings();

        room.ChangeSettings(settings);

        RoomMember secondMember = room.Members[SecondSeatIndex];
        Assert.Equal(SecondSeatIndex, secondMember.SeatIndex);
    }

    [Fact]
    public void Remove_Host_ThrowsInvalidOperation()
    {
        WaitingRoom room = RoomSampleFactory.CreateTwoPlayerRoom();

        Action removeHost = () => room.Remove(RoomSampleFactory.HostAccountId);

        Assert.Throws<InvalidOperationException>(removeHost);
    }

    private static void AdmitGuest(WaitingRoom room)
    {
        var guest = new RoomMember(GuestAccountId, GuestNickname);
        room.Admit(guest);
    }

    private static void AdmitThird(WaitingRoom room)
    {
        var third = new RoomMember(ThirdAccountId, ThirdNickname);
        room.Admit(third);
    }
}
