using System;
using Bastion.Domain.Rooms;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Rooms;

/// <inheritdoc/>
public sealed class PrivateRoomService : IPrivateRoomService
{
    private readonly IRoomPlayerVerifier _playerVerifier;
    private readonly IRoomSettingsValidator _settingsValidator;
    private readonly IRoomKeeper _roomKeeper;

    public PrivateRoomService(
        IRoomPlayerVerifier playerVerifier,
        IRoomSettingsValidator settingsValidator,
        IRoomKeeper roomKeeper)
    {
        ArgumentNullException.ThrowIfNull(playerVerifier);
        ArgumentNullException.ThrowIfNull(settingsValidator);
        ArgumentNullException.ThrowIfNull(roomKeeper);

        _playerVerifier = playerVerifier;
        _settingsValidator = settingsValidator;
        _roomKeeper = roomKeeper;
    }

    /// <inheritdoc/>
    public RoomSnapshot CreateRoom(string sessionToken, RequestedRoomSettings requestedSettings, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(requestedSettings);

        ConnectedPlayer host = _playerVerifier.VerifyPlayerCanEnterRoom(sessionToken, now);
        RoomSettings settings = _settingsValidator.Validate(requestedSettings);
        return _roomKeeper.Open(host, settings, now);
    }

    /// <inheritdoc/>
    public RoomSnapshot JoinRoom(string sessionToken, string code, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(code);

        ConnectedPlayer player = _playerVerifier.VerifyPlayerCanEnterRoom(sessionToken, now);
        string normalizedCode = RoomPolicy.NormalizeCode(code);
        return _roomKeeper.Join(player, normalizedCode, now);
    }

    /// <inheritdoc/>
    public RoomSnapshot SetReady(string sessionToken, bool isReady, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        ConnectedPlayer player = _playerVerifier.VerifySession(sessionToken, now);
        return _roomKeeper.SetReady(player.AccountId, isReady);
    }

    /// <inheritdoc/>
    public RoomSnapshot ChangeSettings(string sessionToken, RequestedRoomSettings requestedSettings, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(requestedSettings);

        ConnectedPlayer host = _playerVerifier.VerifySession(sessionToken, now);
        RoomSettings settings = _settingsValidator.Validate(requestedSettings);
        return _roomKeeper.ChangeSettings(host.AccountId, settings, now);
    }

    /// <inheritdoc/>
    public void LeaveRoom(string sessionToken, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        ConnectedPlayer player = _playerVerifier.VerifySession(sessionToken, now);
        _roomKeeper.Leave(player.AccountId, now);
    }

    /// <inheritdoc/>
    public RoomSnapshot? FindOwnRoom(string sessionToken, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        ConnectedPlayer player = _playerVerifier.VerifySession(sessionToken, now);
        return _roomKeeper.FindRoomOf(player.AccountId);
    }
}
