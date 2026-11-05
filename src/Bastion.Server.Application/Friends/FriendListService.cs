using System;
using System.Linq;
using Bastion.Domain.Friends;
using Bastion.Server.Application.Common;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Friends;

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

    public FriendRoster ListFriends(string sessionToken, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        PlayerIdentity? ownIdentity = _unitOfWork.Accounts.FindPlayerIdentity(player.AccountId);
        if (ownIdentity is null)
        {
            throw new InvalidSessionException();
        }

        var friends = _unitOfWork.Friendships.ListFriends(player.AccountId)
            .Select(ToFriendStatus)
            .OrderBy(friend => friend.Summary.Friend.Nickname, StringComparer.OrdinalIgnoreCase)
            .ToList();
        return new FriendRoster(ownIdentity.FriendCode, friends, now);
    }

    public void RemoveFriend(string sessionToken, string friendCode, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(friendCode);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        string normalizedCode = FriendCodeRules.NormalizeFriendCode(friendCode);
        FriendSummary? friend = _unitOfWork.Friendships.ListFriends(player.AccountId)
            .FirstOrDefault(summary => summary.Friend.FriendCode == normalizedCode);
        if (friend is null || !_unitOfWork.Friendships.Remove(player.AccountId, friend.Friend.AccountId))
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
