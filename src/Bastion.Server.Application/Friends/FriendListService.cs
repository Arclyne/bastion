using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Friends;
using Bastion.Server.Application.Accounts;
using Bastion.Server.Application.Common;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Friends;

/// <inheritdoc/>
public sealed class FriendListService : IFriendListService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISessionVerifier _sessionVerifier;
    private readonly IPlayerPresence _playerPresence;

    public FriendListService(IUnitOfWork unitOfWork, ISessionVerifier sessionVerifier, IPlayerPresence playerPresence)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(sessionVerifier);
        ArgumentNullException.ThrowIfNull(playerPresence);

        _unitOfWork = unitOfWork;
        _sessionVerifier = sessionVerifier;
        _playerPresence = playerPresence;
    }

    /// <inheritdoc/>
    public FriendRoster ListFriends(string sessionToken, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        PlayerIdentity player = _sessionVerifier.VerifyIdentity(sessionToken, now);
        IReadOnlyList<FriendSummary> friendSummaries = _unitOfWork.Friendships.ListFriends(player.AccountId);
        IEnumerable<FriendStatus> friends = friendSummaries
            .Select(ToFriendStatus)
            .OrderBy(friend => friend.Summary.Friend.Nickname, StringComparer.OrdinalIgnoreCase);
        var friendList = new List<FriendStatus>(friends);
        return new FriendRoster(player.FriendCode, friendList, now);
    }

    /// <inheritdoc/>
    public void RemoveFriend(string sessionToken, string friendCode, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(friendCode);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        string normalizedCode = FriendCodePolicy.Normalize(friendCode);
        FriendSummary? friend = _unitOfWork.Friendships.ListFriends(player.AccountId)
            .FirstOrDefault(summary => summary.Friend.FriendCode == normalizedCode);
        if (friend is null)
        {
            throw new FriendshipRejectedException(FriendshipRejection.NotFriends);
        }

        bool isRemoved = _unitOfWork.Friendships.Remove(player.AccountId, friend.Friend.AccountId);
        if (!isRemoved)
        {
            throw new FriendshipRejectedException(FriendshipRejection.NotFriends);
        }
    }

    private FriendStatus ToFriendStatus(FriendSummary summary)
    {
        bool isConnected = _playerPresence.IsPlayerConnected(summary.Friend.AccountId);
        FriendPresence presence = isConnected ? FriendPresence.Connected : FriendPresence.Disconnected;
        return new FriendStatus(summary, presence);
    }
}
