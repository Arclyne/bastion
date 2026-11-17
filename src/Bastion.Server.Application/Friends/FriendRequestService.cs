using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Accounts;
using Bastion.Domain.Common;
using Bastion.Domain.Friends;
using Bastion.Server.Application.Accounts;
using Bastion.Server.Application.Common;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Friends;

/// <inheritdoc/>
public sealed class FriendRequestService : IFriendRequestService
{
    private const int MaximumSearchResults = 20;

    private readonly IUnitOfWork _unitOfWork;
    private readonly ISessionVerifier _sessionVerifier;
    private readonly IFriendNotifier _friendNotifier;

    public FriendRequestService(
        IUnitOfWork unitOfWork,
        ISessionVerifier sessionVerifier,
        IFriendNotifier friendNotifier)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(sessionVerifier);
        ArgumentNullException.ThrowIfNull(friendNotifier);

        _unitOfWork = unitOfWork;
        _sessionVerifier = sessionVerifier;
        _friendNotifier = friendNotifier;
    }

    /// <inheritdoc/>
    public IReadOnlyList<PlayerSearchResult> SearchPlayers(string sessionToken, string text, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(text);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        string searchText = text.Trim();
        if (searchText.Length < AccountPolicy.MinimumNicknameLength)
        {
            return Array.Empty<PlayerSearchResult>();
        }

        FriendRelationshipResolver resolver = CreateRelationshipResolver(player.AccountId, now);
        IReadOnlyList<PlayerIdentity> foundPlayers = FindPlayers(searchText);
        IEnumerable<PlayerSearchResult> results = foundPlayers
            .Select(found => new PlayerSearchResult(found, resolver.Resolve(found.AccountId)));
        return new List<PlayerSearchResult>(results);
    }

    /// <inheritdoc/>
    public FriendRequestOutcome SendFriendRequest(string sessionToken, string friendCode, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(friendCode);

        PlayerIdentity sender = _sessionVerifier.VerifyIdentity(sessionToken, now);
        PlayerIdentity recipient = FindPlayerByCode(friendCode);
        if (recipient.AccountId == sender.AccountId)
        {
            throw new FriendshipRejectedException(FriendshipRejection.OwnAccount);
        }

        IReadOnlyList<int> ownFriendAccountIds = _unitOfWork.Friendships.ListFriendAccountIds(sender.AccountId);
        if (ownFriendAccountIds.Contains(recipient.AccountId))
        {
            throw new FriendshipRejectedException(FriendshipRejection.AlreadyFriends);
        }

        Lookup<FriendRequest> foundPendingRequest = FindPendingRequestAndRemoveExpired(
            sender.AccountId,
            recipient.AccountId,
            now);
        if (foundPendingRequest.IsFound && foundPendingRequest.GetValue().IsSentBy(sender.AccountId))
        {
            throw new FriendshipRejectedException(FriendshipRejection.RequestAlreadySent);
        }

        EnsureBelowFriendLimit(ownFriendAccountIds.Count, recipient.AccountId);
        if (foundPendingRequest.IsFound)
        {
            FriendRequest pendingRequest = foundPendingRequest.GetValue();
            StartFriendship(pendingRequest, sender, now);
            return FriendRequestOutcome.FriendshipStarted;
        }

        var request = new FriendRequest(sender.AccountId, recipient.AccountId, now);
        _unitOfWork.FriendRequests.Add(request);
        _unitOfWork.SaveChanges();
        DateTime expiresAt = request.GetExpirationTime();
        var requestSummary = new FriendRequestSummary(sender, request.RequestedAt, expiresAt);
        _friendNotifier.NotifyFriendRequestReceived(recipient.AccountId, requestSummary);
        return FriendRequestOutcome.RequestSent;
    }

    /// <inheritdoc/>
    public FriendRequestBoard ListFriendRequests(string sessionToken, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        IReadOnlyList<FriendRequestSummary> allReceivedRequests = _unitOfWork.FriendRequests.ListReceived(
            player.AccountId);
        IReadOnlyList<FriendRequestSummary> receivedRequests = ListUnexpired(allReceivedRequests, now);
        IReadOnlyList<FriendRequestSummary> allSentRequests = _unitOfWork.FriendRequests.ListSent(player.AccountId);
        IReadOnlyList<FriendRequestSummary> sentRequests = ListUnexpired(allSentRequests, now);
        return new FriendRequestBoard(receivedRequests, sentRequests, now);
    }

    /// <inheritdoc/>
    public void AcceptFriendRequest(string sessionToken, string requesterFriendCode, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(requesterFriendCode);

        PlayerIdentity recipient = _sessionVerifier.VerifyIdentity(sessionToken, now);
        PlayerIdentity requester = FindPlayerByCode(requesterFriendCode);
        FriendRequest request = FindRequestSentBy(requester.AccountId, recipient.AccountId, now);
        int ownFriendCount = _unitOfWork.Friendships.ListFriendAccountIds(recipient.AccountId).Count;
        EnsureBelowFriendLimit(ownFriendCount, requester.AccountId);
        StartFriendship(request, recipient, now);
    }

    /// <inheritdoc/>
    public void RejectFriendRequest(string sessionToken, string requesterFriendCode, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(requesterFriendCode);

        ConnectedPlayer recipient = _sessionVerifier.Verify(sessionToken, now);
        PlayerIdentity requester = FindPlayerByCode(requesterFriendCode);
        RemoveRequest(requester.AccountId, recipient.AccountId, now);
    }

    /// <inheritdoc/>
    public void CancelFriendRequest(string sessionToken, string recipientFriendCode, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(recipientFriendCode);

        ConnectedPlayer requester = _sessionVerifier.Verify(sessionToken, now);
        PlayerIdentity recipient = FindPlayerByCode(recipientFriendCode);
        RemoveRequest(requester.AccountId, recipient.AccountId, now);
    }

    private FriendRelationshipResolver CreateRelationshipResolver(int accountId, DateTime now)
    {
        IReadOnlyList<int> friendAccountIds = _unitOfWork.Friendships.ListFriendAccountIds(accountId);
        IReadOnlyList<FriendRequest> involvedRequests = _unitOfWork.FriendRequests.ListInvolving(accountId);
        IEnumerable<FriendRequest> unexpiredRequests = involvedRequests.Where(request => !request.IsExpiredAt(now));
        var pendingRequests = new List<FriendRequest>(unexpiredRequests);
        return new FriendRelationshipResolver(accountId, friendAccountIds, pendingRequests);
    }

    private IReadOnlyList<PlayerIdentity> FindPlayers(string searchText)
    {
        Lookup<PlayerIdentity> foundCodeOwner = FriendCodeLookup.FindPlayer(_unitOfWork.Accounts, searchText);
        if (foundCodeOwner.IsFound)
        {
            PlayerIdentity codeOwner = foundCodeOwner.GetValue();
            return new[] { codeOwner };
        }

        return _unitOfWork.Accounts.SearchPlayerIdentities(searchText, MaximumSearchResults);
    }

    private PlayerIdentity FindPlayerByCode(string friendCode)
    {
        Lookup<PlayerIdentity> foundPlayer = FriendCodeLookup.FindPlayer(_unitOfWork.Accounts, friendCode);
        if (!foundPlayer.IsFound)
        {
            throw new FriendshipRejectedException(FriendshipRejection.PlayerNotFound);
        }

        return foundPlayer.GetValue();
    }

    private Lookup<FriendRequest> FindPendingRequestAndRemoveExpired(int accountId, int otherAccountId, DateTime now)
    {
        Lookup<FriendRequest> foundRequest = _unitOfWork.FriendRequests.FindBetween(accountId, otherAccountId);
        if (!foundRequest.IsFound || !foundRequest.GetValue().IsExpiredAt(now))
        {
            return foundRequest;
        }

        FriendRequest expiredRequest = foundRequest.GetValue();
        _unitOfWork.FriendRequests.Remove(expiredRequest);
        return Lookup.Missing<FriendRequest>($"AccountId={accountId}; OtherAccountId={otherAccountId}");
    }

    private void EnsureBelowFriendLimit(int ownFriendCount, int otherAccountId)
    {
        if (ownFriendCount >= FriendshipPolicy.MaximumFriends)
        {
            throw new FriendshipRejectedException(FriendshipRejection.FriendLimitReached);
        }

        int otherFriendCount = _unitOfWork.Friendships.ListFriendAccountIds(otherAccountId).Count;
        if (otherFriendCount >= FriendshipPolicy.MaximumFriends)
        {
            throw new FriendshipRejectedException(FriendshipRejection.RecipientFriendLimitReached);
        }
    }

    private void StartFriendship(FriendRequest request, PlayerIdentity accepter, DateTime now)
    {
        var friendship = new Friendship(request.RequesterId, request.RecipientId, now);
        _unitOfWork.Friendships.Add(friendship);
        _unitOfWork.SaveChanges();
        _unitOfWork.FriendRequests.Remove(request);
        _friendNotifier.NotifyFriendshipStarted(request.RequesterId, accepter);
    }

    private static IReadOnlyList<FriendRequestSummary> ListUnexpired(
        IReadOnlyList<FriendRequestSummary> requests,
        DateTime now)
    {
        IEnumerable<FriendRequestSummary> unexpiredRequests = requests.Where(request => request.ExpiresAt > now);
        return new List<FriendRequestSummary>(unexpiredRequests);
    }

    private void RemoveRequest(int requesterAccountId, int recipientAccountId, DateTime now)
    {
        FriendRequest request = FindRequestSentBy(requesterAccountId, recipientAccountId, now);
        _unitOfWork.FriendRequests.Remove(request);
    }

    private FriendRequest FindRequestSentBy(int requesterAccountId, int recipientAccountId, DateTime now)
    {
        Lookup<FriendRequest> foundRequest = _unitOfWork.FriendRequests.FindBetween(
            requesterAccountId,
            recipientAccountId);
        if (!foundRequest.IsFound)
        {
            throw new FriendshipRejectedException(FriendshipRejection.RequestNotFound);
        }

        FriendRequest request = foundRequest.GetValue();
        if (!request.IsSentBy(requesterAccountId) || request.IsExpiredAt(now))
        {
            throw new FriendshipRejectedException(FriendshipRejection.RequestNotFound);
        }

        return request;
    }
}
