using System;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Matches;

/// <summary>
/// Lets the two players of a match agree to end it in a draw.
/// </summary>
public interface IMatchDrawService
{
    /// <summary>
    /// Offers a draw to the rival of the player of the session, without stopping the match or the clocks. If the rival
    /// had already offered one, the offer is taken as the agreement and the match ends in a draw.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="matchId">The match the player plays.</param>
    /// <param name="now">The current UTC time.</param>
    /// <exception cref="ArgumentNullException"><paramref name="sessionToken"/> is null.</exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    /// <exception cref="MatchNotJoinedException">The player is not playing that match, or the match is over.</exception>
    /// <exception cref="DrawOfferRejectedException">
    /// The match has four players, the player already has an offer pending, or the player offered too recently.
    /// </exception>
    /// <exception cref="MatchRepositoryException">The match could not be read or saved.</exception>
    void OfferDraw(string sessionToken, int matchId, DateTime now);

    /// <summary>
    /// Accepts or rejects the draw the rival offered. Accepting ends the match in a draw, with the elo and statistics
    /// of a ranked match; rejecting tells the rival and the match goes on.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="response">The match and whether the player accepts.</param>
    /// <param name="now">The current UTC time.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sessionToken"/> or <paramref name="response"/> is null.
    /// </exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    /// <exception cref="MatchNotJoinedException">The player is not playing that match, or the match is over.</exception>
    /// <exception cref="DrawNotPendingException">The rival has no draw offer pending.</exception>
    /// <exception cref="MatchRepositoryException">The match could not be read or saved.</exception>
    void RespondToDraw(string sessionToken, DrawResponse response, DateTime now);
}
