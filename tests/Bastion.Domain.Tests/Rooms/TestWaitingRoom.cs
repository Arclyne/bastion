using System;
using System.Linq;
using Bastion.Domain.Rooms;
using Xunit;

namespace Bastion.Domain.Tests.Rooms;

public class TestWaitingRoom
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
        WaitingRoom room = RoomSamples.CreateTwoPlayerRoom();

        RoomMember host = room.Members.Single();
        Assert.Equal((FirstSeatIndex, true), (host.SeatIndex, host.IsReady));
    }

    [Fact]
    public void Admit_Guest_TakesNextSeatNotReady()
    {
        WaitingRoom room = RoomSamples.CreateTwoPlayerRoom();
        var guest = new RoomMember(GuestAccountId, GuestNickname);

        room.Admit(guest);

        Assert.Equal((SecondSeatIndex, false), (guest.SeatIndex, guest.IsReady));
    }

    [Fact]
    public void Admit_FullRoom_ThrowsInvalidOperation()
    {
        WaitingRoom room = RoomSamples.CreateTwoPlayerRoom();
        room.Admit(new RoomMember(GuestAccountId, GuestNickname));

        Assert.Throws<InvalidOperationException>(() => room.Admit(new RoomMember(ThirdAccountId, ThirdNickname)));
    }

    [Fact]
    public void Admit_AfterMemberLeft_TakesFreedSeat()
    {
        WaitingRoom room = RoomSamples.CreateFourPlayerRoom();
        room.Admit(new RoomMember(GuestAccountId, GuestNickname));
        room.Admit(new RoomMember(ThirdAccountId, ThirdNickname));
        room.Remove(GuestAccountId);
        var fourth = new RoomMember(FourthAccountId, FourthNickname);

        room.Admit(fourth);

        Assert.Equal(SecondSeatIndex, fourth.SeatIndex);
    }

    [Fact]
    public void IsReadyToStart_AllSeatsTakenAndReady_ReturnsTrue()
    {
        WaitingRoom room = RoomSamples.CreateTwoPlayerRoom();
        room.Admit(new RoomMember(GuestAccountId, GuestNickname));
        room.SetReady(GuestAccountId, true);

        bool isReadyToStart = room.IsReadyToStart();

        Assert.True(isReadyToStart);
    }

    [Fact]
    public void IsReadyToStart_MemberNotReady_ReturnsFalse()
    {
        WaitingRoom room = RoomSamples.CreateTwoPlayerRoom();
        room.Admit(new RoomMember(GuestAccountId, GuestNickname));

        bool isReadyToStart = room.IsReadyToStart();

        Assert.False(isReadyToStart);
    }

    [Fact]
    public void IsReadyToStart_FreeSeat_ReturnsFalse()
    {
        WaitingRoom room = RoomSamples.CreateFourPlayerRoom();
        room.Admit(new RoomMember(GuestAccountId, GuestNickname));
        room.SetReady(GuestAccountId, true);

        bool isReadyToStart = room.IsReadyToStart();

        Assert.False(isReadyToStart);
    }

    [Fact]
    public void SetReady_HostNotReady_KeepsHostReady()
    {
        WaitingRoom room = RoomSamples.CreateTwoPlayerRoom();

        room.SetReady(RoomSamples.HostAccountId, false);

        Assert.True(room.Members.Single().IsReady);
    }

    [Fact]
    public void ChangeSettings_ReadyGuest_ClearsGuestReady()
    {
        WaitingRoom room = RoomSamples.CreateTwoPlayerRoom();
        room.Admit(new RoomMember(GuestAccountId, GuestNickname));
        room.SetReady(GuestAccountId, true);

        room.ChangeSettings(RoomSamples.CreateTwoPlayerSettings());

        Assert.False(room.Members[SecondSeatIndex].IsReady);
    }

    [Fact]
    public void ChangeSettings_FewerSeatsThanMembers_ThrowsInvalidOperation()
    {
        WaitingRoom room = RoomSamples.CreateFourPlayerRoom();
        room.Admit(new RoomMember(GuestAccountId, GuestNickname));
        room.Admit(new RoomMember(ThirdAccountId, ThirdNickname));

        Assert.Throws<InvalidOperationException>(() => room.ChangeSettings(RoomSamples.CreateTwoPlayerSettings()));
    }

    [Fact]
    public void ChangeSettings_FourToTwoSeats_SeatsMembersTogether()
    {
        WaitingRoom room = RoomSamples.CreateFourPlayerRoom();
        room.Admit(new RoomMember(GuestAccountId, GuestNickname));
        room.Admit(new RoomMember(ThirdAccountId, ThirdNickname));
        room.Remove(GuestAccountId);

        room.ChangeSettings(RoomSamples.CreateTwoPlayerSettings());

        Assert.Equal(SecondSeatIndex, room.Members[SecondSeatIndex].SeatIndex);
    }

    [Fact]
    public void Remove_Host_ThrowsInvalidOperation()
    {
        WaitingRoom room = RoomSamples.CreateTwoPlayerRoom();

        Assert.Throws<InvalidOperationException>(() => room.Remove(RoomSamples.HostAccountId));
    }
}
