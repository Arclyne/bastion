using System;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Matchmaking;

/// <summary>
/// Puts registered players in the ranked matchmaking queue and takes them out of it.
/// </summary>
public interface IMatchSearchService
{
    /// <summary>
    /// Starts a ranked search for the player of the session. A repeated request keeps the first search.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="now">The current UTC time, used as the start of the search.</param>
    /// <exception cref="ArgumentNullException"><paramref name="sessionToken"/> is null.</exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    /// <exception cref="SearchRejectedException">The player cannot search; the rejection says why.</exception>
    void Start(string sessionToken, DateTime now);

    /// <summary>
    /// Ends the player's search and tells the player that it was cancelled. Does nothing if the player is not searching.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="now">The current UTC time.</param>
    /// <exception cref="ArgumentNullException"><paramref name="sessionToken"/> is null.</exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    void Cancel(string sessionToken, DateTime now);

    /// <summary>
    /// Confirms that the player is still waiting, so the search goes on. A late confirmation is ignored.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="now">The current UTC time, from which the next presence check is counted.</param>
    /// <exception cref="ArgumentNullException"><paramref name="sessionToken"/> is null.</exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    void ConfirmPresence(string sessionToken, DateTime now);
}
