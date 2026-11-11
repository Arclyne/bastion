using System.ServiceModel;
using Bastion.Contracts.Sessions;

namespace Bastion.Contracts.Matchmaking;

/// <summary>
/// Puts a logged-in player in the ranked matchmaking queue and takes them out of it.
/// The outcome of a search arrives through the notification channel opened with <see cref="ISessionService"/>.
/// </summary>
[ServiceContract]
public interface IMatchmakingService
{
    /// <summary>
    /// Starts a ranked search for the player of the session. A repeated request keeps the first search.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidSessionFault"/>: the session is not open.
    /// </exception>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="SearchRejectedFault"/>: the player has no notification channel, has an unfinished match, or is
    /// in a room.
    /// </exception>
    /// <exception cref="FaultException">
    /// The server could not reach the database to check the session or the unfinished matches of the player.
    /// </exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(SearchRejectedFault))]
    void StartSearch(string? sessionToken);

    /// <summary>
    /// Ends the player's search. The confirmation arrives as a search-ended notification.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    [OperationContract(IsOneWay = true)]
    void CancelSearch(string? sessionToken);

    /// <summary>
    /// Answers the presence check, so the search goes on. A late answer is ignored.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    [OperationContract(IsOneWay = true)]
    void ConfirmPresence(string? sessionToken);
}
