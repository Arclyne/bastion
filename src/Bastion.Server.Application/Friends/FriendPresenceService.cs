using System;
using System.Collections.Generic;
using Bastion.Domain.Accounts;
using Bastion.Server.Application.Accounts;
using Bastion.Server.Application.Common;

namespace Bastion.Server.Application.Friends;

/// <inheritdoc/>
public sealed class FriendPresenceService : IFriendPresenceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFriendNotifier _friendNotifier;

    public FriendPresenceService(IUnitOfWork unitOfWork, IFriendNotifier friendNotifier)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(friendNotifier);

        _unitOfWork = unitOfWork;
        _friendNotifier = friendNotifier;
    }

    /// <inheritdoc/>
    public void ReportConnected(int accountId)
    {
        PlayerIdentity? identity = _unitOfWork.Accounts.FindPlayerIdentity(accountId);
        if (identity is null)
        {
            return;
        }

        var change = new FriendPresenceChange(identity, FriendPresence.Connected, lastSeenAt: null);
        NotifyFriends(change);
    }

    /// <inheritdoc/>
    public void ReportDisconnected(int accountId, DateTime now)
    {
        Account? account = _unitOfWork.Accounts.FindById(accountId);
        if (account is null)
        {
            return;
        }

        account.RecordLastSeen(now);
        _unitOfWork.SaveChanges();

        var identity = PlayerIdentity.FromAccount(account);
        var change = new FriendPresenceChange(identity, FriendPresence.Disconnected, now);
        NotifyFriends(change);
    }

    private void NotifyFriends(FriendPresenceChange change)
    {
        IReadOnlyList<int> friendAccountIds = _unitOfWork.Friendships.ListFriendAccountIds(change.Friend.AccountId);
        if (friendAccountIds.Count == 0)
        {
            return;
        }

        _friendNotifier.NotifyPresenceChanged(friendAccountIds, change);
    }
}
