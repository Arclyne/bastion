using System;
using Bastion.Client.Networking.Rooms;
using Xunit;

namespace Bastion.Client.Tests.Rooms;

public sealed class TestOwnRoom
{
    [Fact]
    public void GetRoom_OutsideRoom_ThrowsInvalidOperationException()
    {
        var ownRoom = OwnRoom.CreateOutsideRoom();
        Func<PrivateRoom> getRoom = ownRoom.GetRoom;

        Assert.Throws<InvalidOperationException>(getRoom);
    }
}
