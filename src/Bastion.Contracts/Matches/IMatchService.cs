using System.ServiceModel;
using Bastion.Contracts.Sessions;

namespace Bastion.Contracts.Matches;

/// <summary>
/// Handles the requests of a logged-in player inside a match.
/// The other players learn the outcome through the notification channel opened with <see cref="ISessionService"/>.
/// </summary>
[ServiceContract]
public interface IMatchService
{
    /// <summary>
    /// Records that the player of the session leaves the match, and that player loses. With two players the rival wins
    /// and the match ends; with four, the match goes on while two players are still in it.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="matchId">The match the player leaves.</param>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidSessionFault"/>: the session is not open.
    /// </exception>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="MatchNotJoinedFault"/>: the player is not playing that match.
    /// </exception>
    /// <exception cref="FaultException">The server could not read or write the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(MatchNotJoinedFault))]
    void AbandonMatch(string? sessionToken, int matchId);

    /// <summary>
    /// Moves the pawn of the player of the session one square, or over the rival pawn, and passes the turn. Both
    /// players learn the new position through the notification channel; with two players, a pawn that reaches its goal
    /// row ends the match.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="move">The match, the expected move number and the destination square in board notation.</param>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidSessionFault"/>: the session is not open.
    /// </exception>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="MatchNotJoinedFault"/>: the player is not playing that match, or the match is over.
    /// </exception>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="NotYourTurnFault"/>: another player has the turn or the move number is not the expected one;
    /// the detail holds the current turn and the expected move number.
    /// </exception>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="IllegalMoveFault"/>: the pawn cannot reach the destination square.
    /// </exception>
    /// <exception cref="FaultException">The server could not read or write the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(MatchNotJoinedFault))]
    [FaultContract(typeof(NotYourTurnFault))]
    [FaultContract(typeof(IllegalMoveFault))]
    void MovePawn(string? sessionToken, PawnMove? move);
}
