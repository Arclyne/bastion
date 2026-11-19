using System;

namespace Bastion.Server.Application.Matches;

/// <summary>
/// Ends the turns whose clock ran out, as the game server checks the clocks of the matches in progress.
/// </summary>
public interface IMatchClockService
{
    /// <summary>
    /// Finds the matches in progress whose player with the turn has no clock left and applies the timeout to each:
    /// with two players the rival wins and the match ends; with four, the player leaves with the worst free place and
    /// the match goes on while two players are still in it. The players learn the result.
    /// </summary>
    /// <param name="now">The current UTC time, compared with the clock of each player who has the turn.</param>
    /// <exception cref="MatchRepositoryException">The matches whose clock ran out could not be read.</exception>
    void ExpireRunOutClocks(DateTime now);

    /// <summary>
    /// Finds the matches in progress where a player has been disconnected for the whole return delay while another
    /// player is still connected, and takes that player out as an abandonment: with two players the rival wins; with
    /// four, the player leaves with the worst free place and the match goes on. The players learn the result.
    /// </summary>
    /// <param name="now">The current UTC time, compared with the start of each disconnection.</param>
    /// <exception cref="MatchRepositoryException">The matches with an absent player could not be read.</exception>
    void TakeOutAbsentPlayers(DateTime now);
}
