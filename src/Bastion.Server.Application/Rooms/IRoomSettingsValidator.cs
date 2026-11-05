using Bastion.Domain.Rooms;

namespace Bastion.Server.Application.Rooms;

public interface IRoomSettingsValidator
{
    RoomSettings Validate(RequestedRoomSettings requestedSettings);
}
