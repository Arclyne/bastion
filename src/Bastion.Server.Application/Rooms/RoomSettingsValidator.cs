using System;
using Bastion.Domain.GameModes;
using Bastion.Domain.Rooms;
using Bastion.Server.Application.Matches;

namespace Bastion.Server.Application.Rooms;

public sealed class RoomSettingsValidator : IRoomSettingsValidator
{
    private readonly IMatchStore _matchStore;

    public RoomSettingsValidator(IMatchStore matchStore)
    {
        ArgumentNullException.ThrowIfNull(matchStore);

        _matchStore = matchStore;
    }

    public RoomSettings Validate(RequestedRoomSettings requestedSettings)
    {
        ArgumentNullException.ThrowIfNull(requestedSettings);

        GameMode? gameMode = _matchStore.FindActiveGameMode(requestedSettings.GameMode);
        if (gameMode is null)
        {
            throw new RoomRejectedException(RoomRejection.InvalidSettings);
        }

        var settings = new RoomSettings(gameMode, requestedSettings.WallsPerPlayer, requestedSettings.ClockMinutes);
        if (!settings.IsValid())
        {
            throw new RoomRejectedException(RoomRejection.InvalidSettings);
        }

        return settings;
    }
}
