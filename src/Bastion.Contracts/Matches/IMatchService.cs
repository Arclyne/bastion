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
    /// and the match ends; with four, the match goes on while two players are still in it. The player who leaves and
    /// the players still in it learn the result through the notification channel.
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
    /// Works out what the player of the session would lose by leaving the match, without recording anything: the elo
    /// the player entered with and the elo that would be left in a ranked match, the place taken in a four-player match
    /// and how many players would still be in it.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="matchId">The match the player thinks of leaving.</param>
    /// <returns>What leaving the match would mean for the player.</returns>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidSessionFault"/>: the session is not open.
    /// </exception>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="MatchNotJoinedFault"/>: the player is not playing that match, or the match is over.
    /// </exception>
    /// <exception cref="FaultException">The server could not read the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(MatchNotJoinedFault))]
    AbandonmentPreview PreviewAbandonment(string? sessionToken, int matchId);

    /// <summary>
    /// Moves the pawn of the player of the session one square, or over the rival pawn, and passes the turn. Every
    /// player of the match learns the new position through the notification channel. A pawn that reaches its goal row
    /// ends a two-player match and takes the best free place in a four-player one; the players learn the result
    /// through the same channel.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="pawnMove">The match, the expected move number and the destination square in board notation.</param>
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
    /// With <see cref="IllegalMoveFault"/>: the move is missing, or the pawn cannot reach the destination square.
    /// </exception>
    /// <exception cref="FaultException">The server could not read or write the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(MatchNotJoinedFault))]
    [FaultContract(typeof(NotYourTurnFault))]
    [FaultContract(typeof(IllegalMoveFault))]
    void MovePawn(string? sessionToken, PawnMove? pawnMove);

    /// <summary>
    /// Places one wall of the player of the session in a groove of the board and passes the turn. Every player of the
    /// match learns the new wall through the notification channel. A wall never overlaps or crosses another one, and
    /// never leaves a player without a way to the goal.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="wall">
    /// The match, the expected move number, the groove in board notation (the square below and to the left of the wall
    /// center) and the orientation.
    /// </param>
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
    /// With <see cref="IllegalWallFault"/>: the player has no walls left, or the wall falls outside the board, takes
    /// a groove already used or would leave a player without a way to the goal; the detail holds the reason and, in
    /// the last case, the turn of that player. A missing wall or an unknown orientation is rejected as outside the
    /// board.
    /// </exception>
    /// <exception cref="FaultException">The server could not read or write the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(MatchNotJoinedFault))]
    [FaultContract(typeof(NotYourTurnFault))]
    [FaultContract(typeof(IllegalWallFault))]
    void PlaceWall(string? sessionToken, WallPlacement? wall);

    /// <summary>
    /// Finds the match the player of the session has not finished, with its whole state, without bringing the player
    /// back to it: the player stays disconnected and the rivals can still claim the win.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <returns>Whether the player has an unfinished match and, if so, its state.</returns>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidSessionFault"/>: the session is not open.
    /// </exception>
    /// <exception cref="FaultException">The server could not read the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    OngoingMatch FindOngoingMatch(string? sessionToken);

    /// <summary>
    /// Brings the player of the session back to the match after a disconnection and tells the other players. The state
    /// holds every move, the pawns, the walls and the clocks with the running turn already taken off. When the match or
    /// the player's part in it ended while the player was away, the state carries how it ended.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="matchId">The match the player returns to.</param>
    /// <returns>The whole state of the match as the server keeps it.</returns>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidSessionFault"/>: the session is not open.
    /// </exception>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="MatchNotJoinedFault"/>: the player never played that match.
    /// </exception>
    /// <exception cref="FaultException">The server could not read or write the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(MatchNotJoinedFault))]
    MatchSnapshot RejoinMatch(string? sessionToken, int matchId);

    /// <summary>
    /// Offers a draw to the rival of the player of the session without stopping the match or the clocks. The rival
    /// learns it through the notification channel. If the rival had already offered one, the offer is taken as the
    /// agreement and the match ends in a draw.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="matchId">The match the player plays.</param>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidSessionFault"/>: the session is not open.
    /// </exception>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="MatchNotJoinedFault"/>: the player is not playing that match, or the match is over.
    /// </exception>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="DrawOfferRejectedFault"/>: the match has four players, the player already has an offer pending,
    /// or the player offered too recently; the detail says why and how many own moves are left.
    /// </exception>
    /// <exception cref="FaultException">The server could not read or write the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(MatchNotJoinedFault))]
    [FaultContract(typeof(DrawOfferRejectedFault))]
    void OfferDraw(string? sessionToken, int matchId);

    /// <summary>
    /// Accepts or rejects the draw the rival offered. Accepting ends the match in a draw; rejecting tells the rival
    /// through the notification channel and the match goes on.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="matchId">The match the player plays.</param>
    /// <param name="isAccepted">Whether the player accepts the draw.</param>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidSessionFault"/>: the session is not open.
    /// </exception>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="MatchNotJoinedFault"/>: the player is not playing that match, or the match is over.
    /// </exception>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="DrawNotPendingFault"/>: the rival has no draw offer pending, for example because it expired.
    /// </exception>
    /// <exception cref="FaultException">The server could not read or write the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(MatchNotJoinedFault))]
    [FaultContract(typeof(DrawNotPendingFault))]
    void RespondToDraw(string? sessionToken, int matchId, bool isAccepted);
}
