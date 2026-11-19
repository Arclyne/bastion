using System;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Rooms;
using Bastion.Contracts.Notifications;
using Bastion.Contracts.Rooms;
using Xunit;

namespace Bastion.Client.Tests.Rooms;

public sealed class TestRoomContractTranslator
{
    [Fact]
    public void ToOwnRoom_NotInRoom_IsOutsideRoom()
    {
        var result = new OwnRoomResult { IsInRoom = false };

        var ownRoom = RoomContractTranslator.ToOwnRoom(result);

        Assert.False(ownRoom.IsInRoom);
    }

    [Fact]
    public void ToOwnRoom_InRoomWithoutRoom_ThrowsServerErrorException()
    {
        var result = new OwnRoomResult { IsInRoom = true };
        Action translate = () => RoomContractTranslator.ToOwnRoom(result);

        Assert.Throws<ServerErrorException>(translate);
    }

    [Fact]
    public void ToCloseReason_Kicked_IsKicked()
    {
        RoomCloseReason reason = RoomContractTranslator.ToCloseReason(RoomClosedReason.Kicked);

        Assert.Equal(RoomCloseReason.Kicked, reason);
    }

    [Fact]
    public void ToRejection_HostNotKickable_IsHostNotKickable()
    {
        RoomRejection rejection = RoomContractTranslator.ToRejection(RoomRejectionReason.HostNotKickable);

        Assert.Equal(RoomRejection.HostNotKickable, rejection);
    }
}
