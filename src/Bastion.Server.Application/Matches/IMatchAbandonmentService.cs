using System;
using Bastion.Domain.Matches;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Matches;

/// <summary>
/// Takes a player out of a match they leave, and ends the match when one player is left.
/// </summary>
public interface IMatchAbandonmentService
{
    /// <summary>
    /// Records the abandonment of the player of the session, who loses. With two players the rival wins and the match
    /// ends; with four, the match goes on while two players are still in it. The player who leaves and the players
    /// still in it are notified, and a match that ends records the elo and statistics if it is ranked.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="matchId">The match the player leaves.</param>
    /// <param name="now">The current UTC time, which becomes the end of the match if it ends.</param>
    /// <exception cref="ArgumentNullException"><paramref name="sessionToken"/> is null.</exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    /// <exception cref="MatchNotJoinedException">
    /// The player is not playing that match, or the match is over.
    /// </exception>
    /// <exception cref="MatchRepositoryException">The match could not be read or saved.</exception>
    void AbandonMatch(string sessionToken, int matchId, DateTime now);

    /// <summary>
    /// Works out what the player of the session would lose by leaving the match, without recording anything: the elo
    /// the player entered with and the elo that would be left in a ranked match, the place taken in a four-player match
    /// and how many players would still be in it. The numbers come from the same rules the closing applies.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="matchId">The match the player thinks of leaving.</param>
    /// <param name="now">The current UTC time, used as the moment of the leaving.</param>
    /// <returns>What leaving the match would mean for the player.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sessionToken"/> is null.</exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    /// <exception cref="MatchNotJoinedException">
    /// The player is not playing that match, or the match is over.
    /// </exception>
    /// <exception cref="MatchRepositoryException">The match could not be read.</exception>
    AbandonmentPreview PreviewAbandonment(string sessionToken, int matchId, DateTime now);
}
