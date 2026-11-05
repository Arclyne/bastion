using System;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Rooms;

/// <summary>
/// Opens private rooms, lets players join them with their code and keeps the members of each room up to date.
/// </summary>
public interface IPrivateRoomService
{
    /// <summary>
    /// Opens a private room hosted by the player of the session, with a new code and the chosen settings.
    /// </summary>
    /// <param name="sessionToken">The token of the host's open session.</param>
    /// <param name="requestedSettings">The game mode, walls per player and clock chosen by the host.</param>
    /// <param name="now">The current UTC time, used as the opening time of the room.</param>
    /// <returns>The opened room, with the host in the first seat.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    /// <exception cref="RoomRejectedException">The player cannot open a room; the rejection says why.</exception>
    /// <exception cref="RoomStoreException">The room could not be saved.</exception>
    RoomSnapshot CreateRoom(string sessionToken, RequestedRoomSettings requestedSettings, DateTime now);

    /// <summary>
    /// Seats the player of the session in the open room with the given code, not ready yet.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="code">The six-character code of the room.</param>
    /// <param name="now">The current UTC time, used to limit the failed codes of the player.</param>
    /// <returns>The room after the player took a seat.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    /// <exception cref="RoomRejectedException">The player cannot join; the rejection says why.</exception>
    RoomSnapshot JoinRoom(string sessionToken, string code, DateTime now);

    /// <summary>
    /// Marks the player of the session as ready or not ready in their room.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="isReady">Whether the player is ready to play.</param>
    /// <param name="now">The current UTC time.</param>
    /// <returns>The room after the change.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sessionToken"/> is null.</exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    /// <exception cref="RoomRejectedException">The player is not in a room.</exception>
    RoomSnapshot SetReady(string sessionToken, bool isReady, DateTime now);

    /// <summary>
    /// Changes the settings of the room hosted by the player of the session. The other members stop being ready.
    /// </summary>
    /// <param name="sessionToken">The token of the host's open session.</param>
    /// <param name="requestedSettings">The new game mode, walls per player and clock.</param>
    /// <param name="now">The current UTC time, used as the last activity of the room.</param>
    /// <returns>The room after the change.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    /// <exception cref="RoomRejectedException">The settings cannot change; the rejection says why.</exception>
    /// <exception cref="RoomStoreException">The room could not be saved.</exception>
    RoomSnapshot ChangeSettings(string sessionToken, RequestedRoomSettings requestedSettings, DateTime now);

    /// <summary>
    /// Takes the player of the session out of their room. When the host leaves, the room closes.
    /// Does nothing if the player is not in a room.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="now">The current UTC time, used as the closing time of the room.</param>
    /// <exception cref="ArgumentNullException"><paramref name="sessionToken"/> is null.</exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    /// <exception cref="RoomStoreException">The closed room could not be saved.</exception>
    void LeaveRoom(string sessionToken, DateTime now);

    /// <summary>
    /// Finds the room of the player of the session, so a client that lost a reply can show where the player is.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="now">The current UTC time.</param>
    /// <returns>The room of the player, or null if the player is not in a room.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sessionToken"/> is null.</exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    RoomSnapshot? FindOwnRoom(string sessionToken, DateTime now);
}
