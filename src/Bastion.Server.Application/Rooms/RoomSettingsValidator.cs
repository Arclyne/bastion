using System;
using Bastion.Domain.Common;
using Bastion.Domain.GameModes;
using Bastion.Domain.Rooms;
using Bastion.Server.Application.Matches;

namespace Bastion.Server.Application.Rooms;

public sealed class RoomSettingsValidator : IRoomSettingsValidator
{
    private readonly IMatchRepository _matchRepository;

    public RoomSettingsValidator(IMatchRepository matchRepository)
    {
        ArgumentNullException.ThrowIfNull(matchRepository);

        _matchRepository = matchRepository;
    }

    public RoomSettings Validate(RequestedRoomSettings requestedSettings)
    {
        ArgumentNullException.ThrowIfNull(requestedSettings);

        Lookup<GameMode> foundGameMode = _matchRepository.FindActiveGameMode(requestedSettings.GameMode);
        if (!foundGameMode.IsFound)
        {
            throw new RoomRejectedException(RoomRejection.InvalidSettings);
        }

        GameMode gameMode = foundGameMode.GetValue();
        var settings = new RoomSettings(gameMode, requestedSettings.WallsPerPlayer, requestedSettings.ClockMinutes);
        if (!settings.IsValid())
        {
            throw new RoomRejectedException(RoomRejection.InvalidSettings);
        }

        return settings;
    }
}
