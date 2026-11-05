using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Friends;

namespace Bastion.Server.Application.Friends;

public sealed class FriendRelationshipResolver
{
    private readonly int _ownAccountId;
    private readonly IReadOnlyList<int> _friendAccountIds;
    private readonly IReadOnlyList<FriendRequest> _pendingRequests;

    public FriendRelationshipResolver(
        int ownAccountId,
        IReadOnlyList<int> friendAccountIds,
        IReadOnlyList<FriendRequest> pendingRequests)
    {
        ArgumentNullException.ThrowIfNull(friendAccountIds);
        ArgumentNullException.ThrowIfNull(pendingRequests);

        _ownAccountId = ownAccountId;
        _friendAccountIds = friendAccountIds;
        _pendingRequests = pendingRequests;
    }

    public FriendRelationship Resolve(int otherAccountId)
    {
        if (otherAccountId == _ownAccountId)
        {
            return FriendRelationship.OwnAccount;
        }

        if (_friendAccountIds.Contains(otherAccountId))
        {
            return FriendRelationship.AlreadyFriends;
        }

        FriendRequest? request = _pendingRequests.FirstOrDefault(pending =>
            pending.RequesterId == otherAccountId || pending.RecipientId == otherAccountId);
        if (request is null)
        {
            return FriendRelationship.None;
        }

        return request.IsSentBy(_ownAccountId) ? FriendRelationship.RequestSent : FriendRelationship.RequestReceived;
    }
}
