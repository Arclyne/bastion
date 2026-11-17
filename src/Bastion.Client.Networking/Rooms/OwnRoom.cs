using System;

namespace Bastion.Client.Networking.Rooms;

public abstract class OwnRoom
{
    private OwnRoom(bool isInRoom)
    {
        IsInRoom = isInRoom;
    }

    public bool IsInRoom { get; }

    public static OwnRoom CreateInRoom(PrivateRoom room)
    {
        ArgumentNullException.ThrowIfNull(room);

        return new FoundOwnRoom(room);
    }

    public static OwnRoom CreateOutsideRoom()
    {
        return new MissingOwnRoom();
    }

    public abstract PrivateRoom GetRoom();

    private sealed class FoundOwnRoom : OwnRoom
    {
        private readonly PrivateRoom _room;

        public FoundOwnRoom(PrivateRoom room)
            : base(isInRoom: true)
        {
            _room = room;
        }

        public override PrivateRoom GetRoom()
        {
            return _room;
        }
    }

    private sealed class MissingOwnRoom : OwnRoom
    {
        public MissingOwnRoom()
            : base(isInRoom: false)
        {
        }

        public override PrivateRoom GetRoom()
        {
            throw new InvalidOperationException("The player is not in a room.");
        }
    }
}
