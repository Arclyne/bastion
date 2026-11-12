using System;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Matches;

/// <summary>
/// Moves the pawn of the player who has the turn, after checking the move against the board rebuilt on the server.
/// </summary>
public interface IPawnMoveService
{
    /// <summary>
    /// Moves the pawn of the player of the session to the chosen square, records the move, passes the turn and
    /// notifies both players. With two players, a pawn that reaches its goal row ends the match.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="move">The match, the move number the player expects and the destination square.</param>
    /// <param name="now">The current UTC time, which closes the player's turn and dates the move.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sessionToken"/> or <paramref name="move"/> is null.
    /// </exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    /// <exception cref="MatchNotJoinedException">
    /// The player is not playing that match, or the match is over.
    /// </exception>
    /// <exception cref="NotYourTurnException">
    /// Another player has the turn or the move number is not the expected one.
    /// </exception>
    /// <exception cref="IllegalMoveException">The pawn cannot reach the destination square.</exception>
    /// <exception cref="MatchRepositoryException">The match could not be read or saved.</exception>
    void MovePawn(string sessionToken, ChosenPawnMove move, DateTime now);
}
