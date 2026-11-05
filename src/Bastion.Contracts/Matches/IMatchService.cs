using System.ServiceModel;
using Bastion.Contracts.Sessions;

namespace Bastion.Contracts.Matches;

/// <summary>
/// Handles the requests of a signed-in player inside a match.
/// The other players learn the outcome through the notification channel opened with <see cref="ISessionService"/>.
/// </summary>
[ServiceContract]
public interface IMatchService
{
    /// <summary>
    /// Ends the match because the player of the session leaves it: that player loses and the rival wins.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="matchId">The match the player leaves.</param>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidSessionFault"/>: the session is not open.
    /// </exception>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="MatchNotJoinedFault"/>: the player is not playing that match.
    /// </exception>
    /// <exception cref="FaultException">The server could not reach the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(MatchNotJoinedFault))]
    void AbandonMatch(string? sessionToken, int matchId);
}
