using System;
using System.Collections.Generic;
using Bastion.Domain.Friends;
using Bastion.Server.Application.Accounts;
using Bastion.Server.Application.Friends;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Application.Tests.Fakes;
using Xunit;

namespace Bastion.Server.Application.Tests.Friends;

public class TestFriendRequestService
{
    private const int AccountId = 3;
    private const string Nickname = "luis_quo";
    private const string FriendCode = "QU7K2P";
    private const string Token = "luis-token";
    private const string UnknownToken = "unknown-token";
    private const int OtherAccountId = 4;
    private const string OtherNickname = "marta_muros";
    private const string OtherFriendCode = "MA3R7T";
    private const string OtherPrefix = "mar";
    private const string UnusedFriendCode = "AB2CD9";
    private const int FirstFillerAccountId = 1000;

    private static readonly DateTime _now = new DateTime(2026, 11, 5, 18, 0, 0, DateTimeKind.Utc);

    private readonly FakeUnitOfWork _unitOfWork = new FakeUnitOfWork();
    private readonly FakeSessionVerifier _sessionVerifier = new FakeSessionVerifier();
    private readonly FakeFriendNotifier _friendNotifier = new FakeFriendNotifier();
    private readonly FriendRequestService _friendRequestService;

    public TestFriendRequestService()
    {
        _friendRequestService = new FriendRequestService(_unitOfWork, _sessionVerifier, _friendNotifier);
        _sessionVerifier.Add(Token, new PlayerIdentity(AccountId, Nickname, FriendCode));
        _unitOfWork.AccountStore.StorePlayerIdentity(new PlayerIdentity(AccountId, Nickname, FriendCode));
        _unitOfWork.AccountStore.StorePlayerIdentity(
            new PlayerIdentity(OtherAccountId, OtherNickname, OtherFriendCode));
    }

    [Fact]
    public void SearchPlayers_InvalidSession_ThrowsInvalidSession()
    {
        Assert.Throws<InvalidSessionException>(
            () => _friendRequestService.SearchPlayers(UnknownToken, OtherPrefix, _now));
    }

    [Fact]
    public void SearchPlayers_ShortText_ReturnsEmptyList()
    {
        IReadOnlyList<PlayerSearchResult> results = _friendRequestService.SearchPlayers(Token, "ma", _now);

        Assert.Empty(results);
    }

    [Fact]
    public void SearchPlayers_NicknamePrefix_ReturnsMatchingPlayer()
    {
        IReadOnlyList<PlayerSearchResult> results = _friendRequestService.SearchPlayers(Token, OtherPrefix, _now);

        Assert.Equal(OtherNickname, results[0].Player.Nickname);
    }

    [Fact]
    public void SearchPlayers_HyphenatedFriendCode_ReturnsCodeOwner()
    {
        IReadOnlyList<PlayerSearchResult> results = _friendRequestService.SearchPlayers(Token, "ma-3r7t", _now);

        Assert.Equal(OtherNickname, results[0].Player.Nickname);
    }

    [Fact]
    public void SearchPlayers_OwnAccount_ReturnsOwnAccountRelationship()
    {
        IReadOnlyList<PlayerSearchResult> results = _friendRequestService.SearchPlayers(Token, "luis", _now);

        Assert.Equal(FriendRelationship.OwnAccount, results[0].Relationship);
    }

    [Fact]
    public void SearchPlayers_Friend_ReturnsAlreadyFriends()
    {
        _unitOfWork.FriendshipStore.Add(new Friendship(AccountId, OtherAccountId, _now));

        IReadOnlyList<PlayerSearchResult> results = _friendRequestService.SearchPlayers(Token, OtherPrefix, _now);

        Assert.Equal(FriendRelationship.AlreadyFriends, results[0].Relationship);
    }

    [Fact]
    public void SearchPlayers_SentRequest_ReturnsRequestSent()
    {
        _unitOfWork.FriendRequestStore.Add(new FriendRequest(AccountId, OtherAccountId, _now.AddDays(-1)));

        IReadOnlyList<PlayerSearchResult> results = _friendRequestService.SearchPlayers(Token, OtherPrefix, _now);

        Assert.Equal(FriendRelationship.RequestSent, results[0].Relationship);
    }

    [Fact]
    public void SearchPlayers_ReceivedRequest_ReturnsRequestReceived()
    {
        _unitOfWork.FriendRequestStore.Add(new FriendRequest(OtherAccountId, AccountId, _now.AddDays(-1)));

        IReadOnlyList<PlayerSearchResult> results = _friendRequestService.SearchPlayers(Token, OtherPrefix, _now);

        Assert.Equal(FriendRelationship.RequestReceived, results[0].Relationship);
    }

    [Fact]
    public void SearchPlayers_ExpiredRequest_ReturnsNone()
    {
        DateTime expiredAt = _now.AddDays(-FriendshipPolicy.RequestLifetimeDays);
        _unitOfWork.FriendRequestStore.Add(new FriendRequest(OtherAccountId, AccountId, expiredAt));

        IReadOnlyList<PlayerSearchResult> results = _friendRequestService.SearchPlayers(Token, OtherPrefix, _now);

        Assert.Equal(FriendRelationship.None, results[0].Relationship);
    }

    [Fact]
    public void SearchPlayers_Stranger_ReturnsNone()
    {
        IReadOnlyList<PlayerSearchResult> results = _friendRequestService.SearchPlayers(Token, OtherPrefix, _now);

        Assert.Equal(FriendRelationship.None, results[0].Relationship);
    }

    [Fact]
    public void SendFriendRequest_InvalidSession_ThrowsInvalidSession()
    {
        Assert.Throws<InvalidSessionException>(
            () => _friendRequestService.SendFriendRequest(UnknownToken, OtherFriendCode, _now));
    }

    [Fact]
    public void SendFriendRequest_UnknownCode_RejectsPlayerNotFound()
    {
        FriendshipRejectedException? rejection = RecordRejection(UnusedFriendCode);

        Assert.Equal(FriendshipRejection.PlayerNotFound, rejection?.Rejection);
    }

    [Fact]
    public void SendFriendRequest_MalformedCode_RejectsPlayerNotFound()
    {
        FriendshipRejectedException? rejection = RecordRejection("no-es-codigo");

        Assert.Equal(FriendshipRejection.PlayerNotFound, rejection?.Rejection);
    }

    [Fact]
    public void SendFriendRequest_OwnCode_RejectsOwnAccount()
    {
        FriendshipRejectedException? rejection = RecordRejection(FriendCode);

        Assert.Equal(FriendshipRejection.OwnAccount, rejection?.Rejection);
    }

    [Fact]
    public void SendFriendRequest_ExistingFriend_RejectsAlreadyFriends()
    {
        _unitOfWork.FriendshipStore.Add(new Friendship(AccountId, OtherAccountId, _now));

        FriendshipRejectedException? rejection = RecordRejection(OtherFriendCode);

        Assert.Equal(FriendshipRejection.AlreadyFriends, rejection?.Rejection);
    }

    [Fact]
    public void SendFriendRequest_PendingOwnRequest_RejectsRequestAlreadySent()
    {
        _unitOfWork.FriendRequestStore.Add(new FriendRequest(AccountId, OtherAccountId, _now.AddDays(-1)));

        FriendshipRejectedException? rejection = RecordRejection(OtherFriendCode);

        Assert.Equal(FriendshipRejection.RequestAlreadySent, rejection?.Rejection);
    }

    [Fact]
    public void SendFriendRequest_CallerAtLimit_RejectsFriendLimitReached()
    {
        StoreFriendsUpToLimit(AccountId);

        FriendshipRejectedException? rejection = RecordRejection(OtherFriendCode);

        Assert.Equal(FriendshipRejection.FriendLimitReached, rejection?.Rejection);
    }

    [Fact]
    public void SendFriendRequest_RecipientAtLimit_RejectsRecipientFriendLimitReached()
    {
        StoreFriendsUpToLimit(OtherAccountId);

        FriendshipRejectedException? rejection = RecordRejection(OtherFriendCode);

        Assert.Equal(FriendshipRejection.RecipientFriendLimitReached, rejection?.Rejection);
    }

    [Fact]
    public void SendFriendRequest_NewPair_ReturnsRequestSent()
    {
        FriendRequestOutcome outcome = _friendRequestService.SendFriendRequest(Token, OtherFriendCode, _now);

        Assert.Equal(FriendRequestOutcome.RequestSent, outcome);
    }

    [Fact]
    public void SendFriendRequest_NewPair_StoresRequest()
    {
        _friendRequestService.SendFriendRequest(Token, OtherFriendCode, _now);

        Assert.NotNull(_unitOfWork.FriendRequestStore.FindBetween(AccountId, OtherAccountId));
    }

    [Fact]
    public void SendFriendRequest_CrossRequest_ReturnsFriendshipStarted()
    {
        _unitOfWork.FriendRequestStore.Add(new FriendRequest(OtherAccountId, AccountId, _now.AddDays(-1)));

        FriendRequestOutcome outcome = _friendRequestService.SendFriendRequest(Token, OtherFriendCode, _now);

        Assert.Equal(FriendRequestOutcome.FriendshipStarted, outcome);
    }

    [Fact]
    public void SendFriendRequest_CrossRequest_StoresFriendship()
    {
        _unitOfWork.FriendRequestStore.Add(new FriendRequest(OtherAccountId, AccountId, _now.AddDays(-1)));

        _friendRequestService.SendFriendRequest(Token, OtherFriendCode, _now);

        Assert.Single(_unitOfWork.FriendshipStore.AddedFriendships);
    }

    [Fact]
    public void SendFriendRequest_CrossRequest_RemovesRequest()
    {
        _unitOfWork.FriendRequestStore.Add(new FriendRequest(OtherAccountId, AccountId, _now.AddDays(-1)));

        _friendRequestService.SendFriendRequest(Token, OtherFriendCode, _now);

        Assert.Null(_unitOfWork.FriendRequestStore.FindBetween(AccountId, OtherAccountId));
    }

    [Fact]
    public void SendFriendRequest_ExpiredOwnRequest_ReplacesRequest()
    {
        DateTime expiredAt = _now.AddDays(-FriendshipPolicy.RequestLifetimeDays);
        _unitOfWork.FriendRequestStore.Add(new FriendRequest(AccountId, OtherAccountId, expiredAt));

        _friendRequestService.SendFriendRequest(Token, OtherFriendCode, _now);

        Assert.Equal(_now, _unitOfWork.FriendRequestStore.FindBetween(AccountId, OtherAccountId)?.RequestedAt);
    }

    [Fact]
    public void SendFriendRequest_NewPair_NotifiesRecipient()
    {
        _friendRequestService.SendFriendRequest(Token, OtherFriendCode, _now);

        Assert.Equal(new[] { OtherAccountId }, _friendNotifier.RequestRecipientAccountIds);
    }

    [Fact]
    public void SendFriendRequest_CrossRequest_NotifiesRequester()
    {
        StoreReceivedRequest(_now.AddDays(-1));

        _friendRequestService.SendFriendRequest(Token, OtherFriendCode, _now);

        Assert.Equal(new[] { OtherAccountId }, _friendNotifier.FriendshipStartedAccountIds);
    }

    [Fact]
    public void ListFriendRequests_ReceivedRequest_ListsReceived()
    {
        StoreReceivedRequest(_now.AddDays(-1));

        FriendRequestBoard board = _friendRequestService.ListFriendRequests(Token, _now);

        Assert.Single(board.Received);
    }

    [Fact]
    public void ListFriendRequests_SentRequest_ReturnsExpiry()
    {
        _unitOfWork.FriendRequestStore.Add(new FriendRequest(AccountId, OtherAccountId, _now));

        FriendRequestBoard board = _friendRequestService.ListFriendRequests(Token, _now);

        Assert.Equal(_now.AddDays(FriendshipPolicy.RequestLifetimeDays), board.Sent[0].ExpiresAt);
    }

    [Fact]
    public void ListFriendRequests_ExpiredRequest_IsLeftOut()
    {
        StoreReceivedRequest(_now.AddDays(-FriendshipPolicy.RequestLifetimeDays));

        FriendRequestBoard board = _friendRequestService.ListFriendRequests(Token, _now);

        Assert.Empty(board.Received);
    }

    [Fact]
    public void AcceptFriendRequest_NoRequest_RejectsRequestNotFound()
    {
        FriendshipRejectedException? rejection = RecordResponseRejection(
            () => _friendRequestService.AcceptFriendRequest(Token, OtherFriendCode, _now));

        Assert.Equal(FriendshipRejection.RequestNotFound, rejection?.Rejection);
    }

    [Fact]
    public void AcceptFriendRequest_OwnSentRequest_RejectsRequestNotFound()
    {
        _unitOfWork.FriendRequestStore.Add(new FriendRequest(AccountId, OtherAccountId, _now.AddDays(-1)));

        FriendshipRejectedException? rejection = RecordResponseRejection(
            () => _friendRequestService.AcceptFriendRequest(Token, OtherFriendCode, _now));

        Assert.Equal(FriendshipRejection.RequestNotFound, rejection?.Rejection);
    }

    [Fact]
    public void AcceptFriendRequest_ExpiredRequest_RejectsRequestNotFound()
    {
        StoreReceivedRequest(_now.AddDays(-FriendshipPolicy.RequestLifetimeDays));

        FriendshipRejectedException? rejection = RecordResponseRejection(
            () => _friendRequestService.AcceptFriendRequest(Token, OtherFriendCode, _now));

        Assert.Equal(FriendshipRejection.RequestNotFound, rejection?.Rejection);
    }

    [Fact]
    public void AcceptFriendRequest_CallerAtLimit_KeepsRequest()
    {
        StoreReceivedRequest(_now.AddDays(-1));
        StoreFriendsUpToLimit(AccountId);

        Record.Exception(() => _friendRequestService.AcceptFriendRequest(Token, OtherFriendCode, _now));

        Assert.NotNull(_unitOfWork.FriendRequestStore.FindBetween(AccountId, OtherAccountId));
    }

    [Fact]
    public void AcceptFriendRequest_PendingRequest_StoresFriendship()
    {
        StoreReceivedRequest(_now.AddDays(-1));

        _friendRequestService.AcceptFriendRequest(Token, OtherFriendCode, _now);

        Assert.Single(_unitOfWork.FriendshipStore.AddedFriendships);
    }

    [Fact]
    public void AcceptFriendRequest_PendingRequest_RemovesRequest()
    {
        StoreReceivedRequest(_now.AddDays(-1));

        _friendRequestService.AcceptFriendRequest(Token, OtherFriendCode, _now);

        Assert.Null(_unitOfWork.FriendRequestStore.FindBetween(AccountId, OtherAccountId));
    }

    [Fact]
    public void AcceptFriendRequest_PendingRequest_NotifiesRequester()
    {
        StoreReceivedRequest(_now.AddDays(-1));

        _friendRequestService.AcceptFriendRequest(Token, OtherFriendCode, _now);

        Assert.Equal(new[] { OtherAccountId }, _friendNotifier.FriendshipStartedAccountIds);
    }

    [Fact]
    public void RejectFriendRequest_PendingRequest_RemovesRequest()
    {
        StoreReceivedRequest(_now.AddDays(-1));

        _friendRequestService.RejectFriendRequest(Token, OtherFriendCode, _now);

        Assert.Null(_unitOfWork.FriendRequestStore.FindBetween(AccountId, OtherAccountId));
    }

    [Fact]
    public void RejectFriendRequest_PendingRequest_SendsNoNotification()
    {
        StoreReceivedRequest(_now.AddDays(-1));

        _friendRequestService.RejectFriendRequest(Token, OtherFriendCode, _now);

        Assert.Empty(_friendNotifier.FriendshipStartedAccountIds);
    }

    [Fact]
    public void CancelFriendRequest_ReceivedRequest_RejectsRequestNotFound()
    {
        StoreReceivedRequest(_now.AddDays(-1));

        FriendshipRejectedException? rejection = RecordResponseRejection(
            () => _friendRequestService.CancelFriendRequest(Token, OtherFriendCode, _now));

        Assert.Equal(FriendshipRejection.RequestNotFound, rejection?.Rejection);
    }

    [Fact]
    public void CancelFriendRequest_SentRequest_RemovesRequest()
    {
        _unitOfWork.FriendRequestStore.Add(new FriendRequest(AccountId, OtherAccountId, _now.AddDays(-1)));

        _friendRequestService.CancelFriendRequest(Token, OtherFriendCode, _now);

        Assert.Null(_unitOfWork.FriendRequestStore.FindBetween(AccountId, OtherAccountId));
    }

    private static FriendshipRejectedException? RecordResponseRejection(Action response)
    {
        return Record.Exception(response) as FriendshipRejectedException;
    }

    private void StoreReceivedRequest(DateTime requestedAt)
    {
        _unitOfWork.FriendRequestStore.Add(new FriendRequest(OtherAccountId, AccountId, requestedAt));
    }

    private FriendshipRejectedException? RecordRejection(string friendCode)
    {
        return Record.Exception(() => _friendRequestService.SendFriendRequest(Token, friendCode, _now))
            as FriendshipRejectedException;
    }

    private void StoreFriendsUpToLimit(int accountId)
    {
        for (int index = 0; index < FriendshipPolicy.MaximumFriends; index++)
        {
            var friend = new PlayerIdentity(FirstFillerAccountId + index, $"amigo_{index}", UnusedFriendCode);
            _unitOfWork.FriendshipStore.Store(accountId, new FriendSummary(friend, null));
        }
    }
}
