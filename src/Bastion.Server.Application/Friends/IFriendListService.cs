using System;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Friends;

/// <summary>
/// Shows players their friends with who is connected, and ends a friendship.
/// </summary>
public interface IFriendListService
{
    /// <summary>
    /// Lists the friends of the player in nickname order, each one marked as connected or not, with the player's own
    /// friend code.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="now">The current UTC time, kept as the moment of the list.</param>
    /// <returns>The player's friend code, the friends with their presence and the moment of the list.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sessionToken"/> is null.</exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    FriendRoster ListFriends(string sessionToken, DateTime now);

    /// <summary>
    /// Ends the friendship between the player and the friend with the given friend code.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="friendCode">The friend code of the friend to remove, with or without the hyphen.</param>
    /// <param name="now">The current UTC time.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sessionToken"/> or <paramref name="friendCode"/> is null.
    /// </exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    /// <exception cref="FriendshipRejectedException">No friend of the player has that friend code.</exception>
    void RemoveFriend(string sessionToken, string friendCode, DateTime now);
}
