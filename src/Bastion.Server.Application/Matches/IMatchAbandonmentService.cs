using System;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Matches;

/// <summary>
/// Takes a player out of a match they leave, and ends the match when one player is left.
/// </summary>
public interface IMatchAbandonmentService
{
    /// <summary>
    /// Records the abandonment of the player of the session, who loses. With two players the rival wins and the match
    /// ends; with four, the match goes on while two players are still in it. The players still in it are notified.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="matchId">The match the player leaves.</param>
    /// <param name="now">The current UTC time, which becomes the end of the match if it ends.</param>
    /// <exception cref="ArgumentNullException"><paramref name="sessionToken"/> is null.</exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    /// <exception cref="MatchNotJoinedException">The player is not playing that match.</exception>
    /// <exception cref="MatchStoreException">The match could not be read or saved.</exception>
    void AbandonMatch(string sessionToken, int matchId, DateTime now);
}
