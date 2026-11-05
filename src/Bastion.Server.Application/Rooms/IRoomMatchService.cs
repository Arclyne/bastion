using System;
using Bastion.Server.Application.Matches;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Rooms;

/// <summary>
/// Starts the private match of a room when its host asks for it.
/// </summary>
public interface IRoomMatchService
{
    /// <summary>
    /// Creates the private match of the room hosted by the player of the session and tells every member,
    /// as long as every seat is taken and every member is ready. The room leaves the server's memory.
    /// </summary>
    /// <param name="sessionToken">The token of the host's open session.</param>
    /// <param name="now">The current UTC time, used as the start of the match.</param>
    /// <returns>The identifier of the created match.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sessionToken"/> is null.</exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    /// <exception cref="RoomRejectedException">The match cannot start; the rejection says why.</exception>
    /// <exception cref="RoomStoreException">The match and the room could not be saved; the room stays open.</exception>
    /// <exception cref="MatchStoreException">
    /// The ratings of the members could not be read; the room stays open.
    /// </exception>
    int StartMatch(string sessionToken, DateTime now);
}
