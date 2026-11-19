using System;
using Bastion.Domain.Common;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Matches;

/// <summary>
/// Follows the connection of the players of a match: marks who drops and brings a player back to the match.
/// </summary>
public interface IMatchConnectionService
{
    /// <summary>
    /// Marks the player as disconnected from the match they are playing, if any, and tells the other players. The
    /// player's clock keeps running when it is their turn.
    /// </summary>
    /// <param name="accountId">The account whose notification channel dropped.</param>
    /// <param name="now">The current UTC time, recorded as the start of the disconnection.</param>
    /// <exception cref="MatchRepositoryException">The match could not be read or saved.</exception>
    void MarkDisconnected(int accountId, DateTime now);

    /// <summary>
    /// Finds the match the player of the session has not finished, without bringing them back to it.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="now">The current UTC time, used to measure the clock of the player in turn.</param>
    /// <returns>The state of the unfinished match, or nothing when the player has none.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sessionToken"/> is null.</exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    /// <exception cref="MatchRepositoryException">The match could not be read.</exception>
    Lookup<MatchSnapshot> FindOngoingMatch(string sessionToken, DateTime now);

    /// <summary>
    /// Brings the player of the session back to the match and tells the other players. A clock that ran out while the
    /// player was away is applied first. When the match or the player's part in it is over, the state says how it ended.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="matchId">The match the player returns to.</param>
    /// <param name="now">The current UTC time.</param>
    /// <returns>The whole state of the match as the server keeps it.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sessionToken"/> is null.</exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    /// <exception cref="MatchNotJoinedException">The player never played that match.</exception>
    /// <exception cref="MatchRepositoryException">The match could not be read or saved.</exception>
    MatchSnapshot RejoinMatch(string sessionToken, int matchId, DateTime now);
}
