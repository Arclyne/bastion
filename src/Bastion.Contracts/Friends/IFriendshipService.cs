using System.ServiceModel;
using Bastion.Contracts.Sessions;

namespace Bastion.Contracts.Friends;

/// <summary>
/// Shows players their friends and friend requests, finds other players, handles friend requests and ends friendships.
/// The other player hears about a new request or friendship through the notification channel opened with
/// <see cref="ISessionService"/>.
/// </summary>
[ServiceContract]
public interface IFriendshipService
{
    /// <summary>
    /// Lists the friends of the player in nickname order, each one marked as connected or not, with the player's own
    /// friend code.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <returns>The player's friend code, the friends with their presence and the server time of the list.</returns>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidSessionFault"/>: the session is not open.
    /// </exception>
    /// <exception cref="FaultException">The server could not reach the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    FriendList ListFriends(string? sessionToken);

    /// <summary>
    /// Finds players by friend code or, if the text is not the code of anyone, by the start of their nickname, and says
    /// how each one relates to the player.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="text">The friend code or the start of the nickname to look for.</param>
    /// <returns>The players found with their relationship, or none if the text is missing or too short.</returns>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidSessionFault"/>: the session is not open.
    /// </exception>
    /// <exception cref="FaultException">The server could not reach the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    PlayerSearchEntry[] SearchPlayers(string? sessionToken, string? text);

    /// <summary>
    /// Sends a friend request to the player with the given friend code, who receives it as a notification. If that
    /// player had already sent one to the sender, the friendship starts instead and that player is notified.
    /// </summary>
    /// <param name="sessionToken">The token of the sender's open session.</param>
    /// <param name="friendCode">The friend code of the recipient, with or without the hyphen.</param>
    /// <returns>Whether the request was sent or the friendship started.</returns>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidSessionFault"/>: the session is not open.
    /// </exception>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="FriendshipRejectedFault"/>: the code is missing or belongs to no player, it is the sender's own
    /// code, they are already friends, the request was already sent, or one of the two has reached the friend limit.
    /// </exception>
    /// <exception cref="FaultException">The server could not read or write the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(FriendshipRejectedFault))]
    FriendRequestResult SendFriendRequest(string? sessionToken, string? friendCode);

    /// <summary>
    /// Lists the friend requests that the player received and sent and that have not expired.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <returns>The received and sent requests and the server time of the list.</returns>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidSessionFault"/>: the session is not open.
    /// </exception>
    /// <exception cref="FaultException">The server could not reach the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    FriendRequestList ListFriendRequests(string? sessionToken);

    /// <summary>
    /// Accepts the friend request that the player with the given friend code sent and starts the friendship. That
    /// player is notified.
    /// </summary>
    /// <param name="sessionToken">The token of the recipient's open session.</param>
    /// <param name="requesterFriendCode">The friend code of the player who sent the request.</param>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidSessionFault"/>: the session is not open.
    /// </exception>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="FriendshipRejectedFault"/>: the code is missing or belongs to no player, there is no unexpired
    /// request from that player, or one of the two has reached the friend limit.
    /// </exception>
    /// <exception cref="FaultException">The server could not read or write the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(FriendshipRejectedFault))]
    void AcceptFriendRequest(string? sessionToken, string? requesterFriendCode);

    /// <summary>
    /// Rejects the friend request that the player with the given friend code sent.
    /// </summary>
    /// <param name="sessionToken">The token of the recipient's open session.</param>
    /// <param name="requesterFriendCode">The friend code of the player who sent the request.</param>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidSessionFault"/>: the session is not open.
    /// </exception>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="FriendshipRejectedFault"/>: the code is missing or belongs to no player, or there is no
    /// unexpired request from that player.
    /// </exception>
    /// <exception cref="FaultException">The server could not read or write the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(FriendshipRejectedFault))]
    void RejectFriendRequest(string? sessionToken, string? requesterFriendCode);

    /// <summary>
    /// Cancels the friend request that the player sent to the player with the given friend code.
    /// </summary>
    /// <param name="sessionToken">The token of the sender's open session.</param>
    /// <param name="recipientFriendCode">The friend code of the player who received the request.</param>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidSessionFault"/>: the session is not open.
    /// </exception>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="FriendshipRejectedFault"/>: the code is missing or belongs to no player, or there is no
    /// unexpired request to that player.
    /// </exception>
    /// <exception cref="FaultException">The server could not read or write the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(FriendshipRejectedFault))]
    void CancelFriendRequest(string? sessionToken, string? recipientFriendCode);

    /// <summary>
    /// Ends the friendship between the player and the friend with the given friend code.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="friendCode">The friend code of the friend to remove, with or without the hyphen.</param>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidSessionFault"/>: the session is not open.
    /// </exception>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="FriendshipRejectedFault"/>: the code is missing, or no friend of the player has that code.
    /// </exception>
    /// <exception cref="FaultException">The server could not read or write the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(FriendshipRejectedFault))]
    void RemoveFriend(string? sessionToken, string? friendCode);
}
