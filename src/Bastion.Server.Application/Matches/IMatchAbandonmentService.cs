using System;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Matches;

/// <summary>
/// Ends a match when one of its players leaves it.
/// </summary>
public interface IMatchAbandonmentService
{
    /// <summary>
    /// Ends the match as an abandonment by the player of the session, who loses; the rival wins and is notified.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="matchId">The match the player leaves.</param>
    /// <param name="now">The current UTC time, which becomes the end of the match.</param>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    /// <exception cref="MatchNotJoinedException">The player is not playing that match.</exception>
    /// <exception cref="MatchStoreException">The match could not be read or saved.</exception>
    void AbandonMatch(string sessionToken, int matchId, DateTime now);
}
