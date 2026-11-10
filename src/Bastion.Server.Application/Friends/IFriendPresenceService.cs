using System;

namespace Bastion.Server.Application.Friends;

/// <summary>
/// Tells the friends of a player when the player connects or disconnects.
/// </summary>
public interface IFriendPresenceService
{
    /// <summary>
    /// Tells the friends of the player that the player is connected. Does nothing for an unknown account.
    /// </summary>
    /// <param name="accountId">The account of the player who connected.</param>
    void ReportConnected(int accountId);

    /// <summary>
    /// Saves when the player was last seen and tells the player's friends that the player is disconnected. Does
    /// nothing for an unknown account.
    /// </summary>
    /// <param name="accountId">The account of the player who disconnected.</param>
    /// <param name="now">The current UTC time, saved as the moment the player was last seen.</param>
    void ReportDisconnected(int accountId, DateTime now);
}
