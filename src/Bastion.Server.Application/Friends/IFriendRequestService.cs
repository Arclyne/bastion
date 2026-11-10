using System;
using System.Collections.Generic;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Friends;

/// <summary>
/// Finds players and handles friend requests: sending, listing, accepting, rejecting and cancelling them.
/// </summary>
public interface IFriendRequestService
{
    /// <summary>
    /// Finds players by friend code or, if the text is not the code of anyone, by the start of their nickname, and
    /// says how each one relates to the player.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="text">The friend code or the start of the nickname to look for.</param>
    /// <param name="now">The current UTC time, used to skip expired requests.</param>
    /// <returns>The players found with their relationship, or an empty list if the text is too short.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sessionToken"/> or <paramref name="text"/> is null.
    /// </exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    IReadOnlyList<PlayerSearchResult> SearchPlayers(string sessionToken, string text, DateTime now);

    /// <summary>
    /// Sends a friend request to the player with the given friend code. If that player had already sent one to the
    /// sender, the friendship starts instead.
    /// </summary>
    /// <param name="sessionToken">The token of the sender's open session.</param>
    /// <param name="friendCode">The friend code of the recipient, with or without the hyphen.</param>
    /// <param name="now">The current UTC time, saved as the moment of the request.</param>
    /// <returns>Whether the request was sent or the friendship started.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sessionToken"/> or <paramref name="friendCode"/> is null.
    /// </exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    /// <exception cref="FriendshipRejectedException">
    /// No player has the code, it is the sender's own code, they are already friends, the request was already sent,
    /// or one of the two has reached the friend limit.
    /// </exception>
    FriendRequestOutcome SendFriendRequest(string sessionToken, string friendCode, DateTime now);

    /// <summary>
    /// Lists the requests that the player received and sent and that have not expired.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="now">The current UTC time, used to leave out expired requests.</param>
    /// <returns>The received and sent requests and the moment of the list.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sessionToken"/> is null.</exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    FriendRequestBoard ListFriendRequests(string sessionToken, DateTime now);

    /// <summary>
    /// Accepts the request that the player with the given friend code sent, and starts the friendship.
    /// </summary>
    /// <param name="sessionToken">The token of the recipient's open session.</param>
    /// <param name="requesterFriendCode">The friend code of the player who sent the request.</param>
    /// <param name="now">The current UTC time, saved as the start of the friendship.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sessionToken"/> or <paramref name="requesterFriendCode"/> is null.
    /// </exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    /// <exception cref="FriendshipRejectedException">
    /// No player has the code, there is no unexpired request from that player, or one of the two has reached the
    /// friend limit.
    /// </exception>
    void AcceptFriendRequest(string sessionToken, string requesterFriendCode, DateTime now);

    /// <summary>
    /// Rejects the request that the player with the given friend code sent.
    /// </summary>
    /// <param name="sessionToken">The token of the recipient's open session.</param>
    /// <param name="requesterFriendCode">The friend code of the player who sent the request.</param>
    /// <param name="now">The current UTC time, used to ignore an expired request.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sessionToken"/> or <paramref name="requesterFriendCode"/> is null.
    /// </exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    /// <exception cref="FriendshipRejectedException">
    /// No player has the code, or there is no unexpired request from that player.
    /// </exception>
    void RejectFriendRequest(string sessionToken, string requesterFriendCode, DateTime now);

    /// <summary>
    /// Cancels the request that the player sent to the player with the given friend code.
    /// </summary>
    /// <param name="sessionToken">The token of the sender's open session.</param>
    /// <param name="recipientFriendCode">The friend code of the player who received the request.</param>
    /// <param name="now">The current UTC time, used to ignore an expired request.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sessionToken"/> or <paramref name="recipientFriendCode"/> is null.
    /// </exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    /// <exception cref="FriendshipRejectedException">
    /// No player has the code, or there is no unexpired request to that player.
    /// </exception>
    void CancelFriendRequest(string sessionToken, string recipientFriendCode, DateTime now);
}
