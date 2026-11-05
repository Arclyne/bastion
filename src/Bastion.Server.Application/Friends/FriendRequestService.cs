using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Accounts;
using Bastion.Domain.Friends;
using Bastion.Server.Application.Common;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Friends;

public sealed class FriendRequestService : IFriendRequestService
{
    private const int MaximumSearchResults = 20;

    private readonly IUnitOfWork _unitOfWork;
    private readonly ISessionVerifier _sessionVerifier;

    public FriendRequestService(IUnitOfWork unitOfWork, ISessionVerifier sessionVerifier)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(sessionVerifier);

        _unitOfWork = unitOfWork;
        _sessionVerifier = sessionVerifier;
    }

    public IReadOnlyList<PlayerSearchResult> SearchPlayers(string sessionToken, string text, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(text);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        string searchText = text.Trim();
        if (searchText.Length < AccountRules.MinimumNicknameLength)
        {
            return Array.Empty<PlayerSearchResult>();
        }

        FriendRelationshipResolver resolver = CreateRelationshipResolver(player.AccountId, now);
        return FindPlayers(searchText)
            .Select(found => new PlayerSearchResult(found, resolver.Resolve(found.AccountId)))
            .ToList();
    }

    public FriendRequestOutcome SendFriendRequest(string sessionToken, string friendCode, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(friendCode);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        PlayerIdentity recipient = FindRecipient(friendCode);
        if (recipient.AccountId == player.AccountId)
        {
            throw new FriendshipRejectedException(FriendshipRejection.OwnAccount);
        }

        IReadOnlyList<int> ownFriendAccountIds = _unitOfWork.Friendships.ListFriendAccountIds(player.AccountId);
        if (ownFriendAccountIds.Contains(recipient.AccountId))
        {
            throw new FriendshipRejectedException(FriendshipRejection.AlreadyFriends);
        }

        FriendRequest? pendingRequest = FindPendingRequest(player.AccountId, recipient.AccountId, now);
        if (pendingRequest is not null && pendingRequest.IsSentBy(player.AccountId))
        {
            throw new FriendshipRejectedException(FriendshipRejection.RequestAlreadySent);
        }

        EnsureBelowFriendLimit(ownFriendAccountIds.Count, recipient.AccountId);
        if (pendingRequest is not null)
        {
            return StartFriendship(pendingRequest, now);
        }

        _unitOfWork.FriendRequests.Add(new FriendRequest(player.AccountId, recipient.AccountId, now));
        _unitOfWork.SaveChanges();
        return FriendRequestOutcome.RequestSent;
    }

    private PlayerIdentity FindRecipient(string friendCode)
    {
        string normalizedCode = FriendCodeRules.NormalizeFriendCode(friendCode);
        PlayerIdentity? recipient = FriendCodeRules.IsValidFriendCode(normalizedCode)
            ? _unitOfWork.Accounts.FindPlayerIdentityByFriendCode(normalizedCode)
            : null;
        return recipient ?? throw new FriendshipRejectedException(FriendshipRejection.PlayerNotFound);
    }

    private FriendRequest? FindPendingRequest(int accountId, int otherAccountId, DateTime now)
    {
        FriendRequest? request = _unitOfWork.FriendRequests.FindBetween(accountId, otherAccountId);
        if (request is null || !request.IsExpiredAt(now))
        {
            return request;
        }

        _unitOfWork.FriendRequests.Remove(request);
        return null;
    }

    private void EnsureBelowFriendLimit(int ownFriendCount, int recipientAccountId)
    {
        if (ownFriendCount >= FriendshipRules.MaximumFriends)
        {
            throw new FriendshipRejectedException(FriendshipRejection.FriendLimitReached);
        }

        if (_unitOfWork.Friendships.ListFriendAccountIds(recipientAccountId).Count >= FriendshipRules.MaximumFriends)
        {
            throw new FriendshipRejectedException(FriendshipRejection.RecipientFriendLimitReached);
        }
    }

    private FriendRequestOutcome StartFriendship(FriendRequest request, DateTime now)
    {
        _unitOfWork.Friendships.Add(new Friendship(request.RequesterId, request.RecipientId, now));
        _unitOfWork.SaveChanges();
        _unitOfWork.FriendRequests.Remove(request);
        return FriendRequestOutcome.FriendshipStarted;
    }

    private IReadOnlyList<PlayerIdentity> FindPlayers(string searchText)
    {
        string normalizedCode = FriendCodeRules.NormalizeFriendCode(searchText);
        if (FriendCodeRules.IsValidFriendCode(normalizedCode))
        {
            PlayerIdentity? codeOwner = _unitOfWork.Accounts.FindPlayerIdentityByFriendCode(normalizedCode);
            if (codeOwner is not null)
            {
                return new[] { codeOwner };
            }
        }

        return _unitOfWork.Accounts.SearchPlayerIdentities(searchText, MaximumSearchResults);
    }

    private FriendRelationshipResolver CreateRelationshipResolver(int accountId, DateTime now)
    {
        IReadOnlyList<int> friendAccountIds = _unitOfWork.Friendships.ListFriendAccountIds(accountId);
        var pendingRequests = _unitOfWork.FriendRequests.ListInvolving(accountId)
            .Where(request => !request.IsExpiredAt(now))
            .ToList();
        return new FriendRelationshipResolver(accountId, friendAccountIds, pendingRequests);
    }
}
