using System;
using System.Collections.Generic;
using Bastion.Domain.Friends;
using Bastion.Server.Application.Accounts;
using Bastion.Server.Application.Friends;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Application.Tests.Fakes;
using Xunit;

namespace Bastion.Server.Application.Tests.Friends;

public sealed class TestFriendRequestService
{
    private const int AccountId = 3;
    private const string Nickname = "luis_quo";
    private const string FriendCode = "QU7K2P";
    private const string Token = "luis-token";
    private const string UnknownToken = "unknown-token";
    private const int OtherAccountId = 4;
    private const string OtherNickname = "marta_muros";
    private const string OtherFriendCode = "MA3R7T";
    private const string HyphenatedOtherFriendCode = "ma-3r7t";
    private const string OtherNicknamePrefix = "mar";
    private const string ShortPrefix = "ma";
    private const string OwnPrefix = "luis";
    private const string UnusedFriendCode = "AB2CD9";
    private const string MalformedFriendCode = "no-es-codigo";
    private const int FirstFillerAccountId = 1000;
    private const string FillerNicknamePrefix = "amigo_";
    private const int DaysAgo = 1;

    private static readonly DateTime _now = new DateTime(2026, 11, 5, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _yesterday = _now.AddDays(-DaysAgo);
    private static readonly DateTime _expiredAt = _now.AddDays(-FriendshipPolicy.RequestLifetimeDays);
    private static readonly PlayerIdentity _caller = new PlayerIdentity(AccountId, Nickname, FriendCode);
    private static readonly PlayerIdentity _otherPlayer = new PlayerIdentity(OtherAccountId, OtherNickname, OtherFriendCode);

    private readonly FakeUnitOfWork _unitOfWork = new FakeUnitOfWork();
    private readonly FakeSessionVerifier _sessionVerifier = new FakeSessionVerifier();
    private readonly FakeFriendNotifier _friendNotifier = new FakeFriendNotifier();
    private readonly FriendRequestService _friendRequestService;

    public TestFriendRequestService()
    {
        _friendRequestService = new FriendRequestService(_unitOfWork, _sessionVerifier, _friendNotifier);
    }

    [Fact]
    public void SearchPlayers_InvalidSession_ThrowsInvalidSession()
    {
        Action search = () => _friendRequestService.SearchPlayers(UnknownToken, OtherNicknamePrefix, _now);

        Assert.Throws<InvalidSessionException>(search);
    }

    [Fact]
    public void SearchPlayers_ShortText_ReturnsEmptyList()
    {
        AddPlayers();

        IReadOnlyList<PlayerSearchResult> results = _friendRequestService.SearchPlayers(Token, ShortPrefix, _now);

        Assert.Empty(results);
    }

    [Fact]
    public void SearchPlayers_NicknamePrefix_ReturnsMatchingPlayer()
    {
        AddPlayers();

        IReadOnlyList<PlayerSearchResult> results = _friendRequestService.SearchPlayers(Token, OtherNicknamePrefix, _now);

        PlayerSearchResult first = results[0];
        Assert.Equal(OtherNickname, first.Player.Nickname);
    }

    [Fact]
    public void SearchPlayers_HyphenatedFriendCode_ReturnsCodeOwner()
    {
        AddPlayers();

        IReadOnlyList<PlayerSearchResult> results =
            _friendRequestService.SearchPlayers(Token, HyphenatedOtherFriendCode, _now);

        PlayerSearchResult first = results[0];
        Assert.Equal(OtherNickname, first.Player.Nickname);
    }

    [Fact]
    public void SearchPlayers_OwnAccount_ReturnsOwnAccountRelationship()
    {
        AddPlayers();

        IReadOnlyList<PlayerSearchResult> results = _friendRequestService.SearchPlayers(Token, OwnPrefix, _now);

        PlayerSearchResult first = results[0];
        Assert.Equal(FriendRelationship.OwnAccount, first.Relationship);
    }

    [Fact]
    public void SearchPlayers_Friend_ReturnsAlreadyFriends()
    {
        AddPlayers();
        AddFriendship();

        IReadOnlyList<PlayerSearchResult> results = _friendRequestService.SearchPlayers(Token, OtherNicknamePrefix, _now);

        PlayerSearchResult first = results[0];
        Assert.Equal(FriendRelationship.AlreadyFriends, first.Relationship);
    }

    [Fact]
    public void SearchPlayers_SentRequest_ReturnsRequestSent()
    {
        AddPlayers();
        AddSentRequest(_yesterday);

        IReadOnlyList<PlayerSearchResult> results = _friendRequestService.SearchPlayers(Token, OtherNicknamePrefix, _now);

        PlayerSearchResult first = results[0];
        Assert.Equal(FriendRelationship.RequestSent, first.Relationship);
    }

    [Fact]
    public void SearchPlayers_ReceivedRequest_ReturnsRequestReceived()
    {
        AddPlayers();
        AddReceivedRequest(_yesterday);

        IReadOnlyList<PlayerSearchResult> results = _friendRequestService.SearchPlayers(Token, OtherNicknamePrefix, _now);

        PlayerSearchResult first = results[0];
        Assert.Equal(FriendRelationship.RequestReceived, first.Relationship);
    }

    [Fact]
    public void SearchPlayers_ExpiredRequest_ReturnsNone()
    {
        AddPlayers();
        AddReceivedRequest(_expiredAt);

        IReadOnlyList<PlayerSearchResult> results = _friendRequestService.SearchPlayers(Token, OtherNicknamePrefix, _now);

        PlayerSearchResult first = results[0];
        Assert.Equal(FriendRelationship.None, first.Relationship);
    }

    [Fact]
    public void SearchPlayers_Stranger_ReturnsNone()
    {
        AddPlayers();

        IReadOnlyList<PlayerSearchResult> results = _friendRequestService.SearchPlayers(Token, OtherNicknamePrefix, _now);

        PlayerSearchResult first = results[0];
        Assert.Equal(FriendRelationship.None, first.Relationship);
    }

    [Fact]
    public void SendFriendRequest_InvalidSession_ThrowsInvalidSession()
    {
        Action send = () => _friendRequestService.SendFriendRequest(UnknownToken, OtherFriendCode, _now);

        Assert.Throws<InvalidSessionException>(send);
    }

    [Fact]
    public void SendFriendRequest_UnknownCode_RejectsPlayerNotFound()
    {
        AddPlayers();
        Action send = () => _friendRequestService.SendFriendRequest(Token, UnusedFriendCode, _now);

        Exception? exception = Record.Exception(send);

        var rejection = exception as FriendshipRejectedException;
        Assert.Equal(FriendshipRejection.PlayerNotFound, rejection?.Rejection);
    }

    [Fact]
    public void SendFriendRequest_MalformedCode_RejectsPlayerNotFound()
    {
        AddPlayers();
        Action send = () => _friendRequestService.SendFriendRequest(Token, MalformedFriendCode, _now);

        Exception? exception = Record.Exception(send);

        var rejection = exception as FriendshipRejectedException;
        Assert.Equal(FriendshipRejection.PlayerNotFound, rejection?.Rejection);
    }

    [Fact]
    public void SendFriendRequest_OwnCode_RejectsOwnAccount()
    {
        AddPlayers();
        Action send = () => _friendRequestService.SendFriendRequest(Token, FriendCode, _now);

        Exception? exception = Record.Exception(send);

        var rejection = exception as FriendshipRejectedException;
        Assert.Equal(FriendshipRejection.OwnAccount, rejection?.Rejection);
    }

    [Fact]
    public void SendFriendRequest_ExistingFriend_RejectsAlreadyFriends()
    {
        AddPlayers();
        AddFriendship();
        Action send = () => _friendRequestService.SendFriendRequest(Token, OtherFriendCode, _now);

        Exception? exception = Record.Exception(send);

        var rejection = exception as FriendshipRejectedException;
        Assert.Equal(FriendshipRejection.AlreadyFriends, rejection?.Rejection);
    }

    [Fact]
    public void SendFriendRequest_PendingOwnRequest_RejectsRequestAlreadySent()
    {
        AddPlayers();
        AddSentRequest(_yesterday);
        Action send = () => _friendRequestService.SendFriendRequest(Token, OtherFriendCode, _now);

        Exception? exception = Record.Exception(send);

        var rejection = exception as FriendshipRejectedException;
        Assert.Equal(FriendshipRejection.RequestAlreadySent, rejection?.Rejection);
    }

    [Fact]
    public void SendFriendRequest_CallerAtLimit_RejectsFriendLimitReached()
    {
        AddPlayers();
        AddFriendsUpToLimit(AccountId);
        Action send = () => _friendRequestService.SendFriendRequest(Token, OtherFriendCode, _now);

        Exception? exception = Record.Exception(send);

        var rejection = exception as FriendshipRejectedException;
        Assert.Equal(FriendshipRejection.FriendLimitReached, rejection?.Rejection);
    }

    [Fact]
    public void SendFriendRequest_RecipientAtLimit_RejectsRecipientFriendLimitReached()
    {
        AddPlayers();
        AddFriendsUpToLimit(OtherAccountId);
        Action send = () => _friendRequestService.SendFriendRequest(Token, OtherFriendCode, _now);

        Exception? exception = Record.Exception(send);

        var rejection = exception as FriendshipRejectedException;
        Assert.Equal(FriendshipRejection.RecipientFriendLimitReached, rejection?.Rejection);
    }

    [Fact]
    public void SendFriendRequest_NewPair_ReturnsRequestSent()
    {
        AddPlayers();

        FriendRequestOutcome outcome = _friendRequestService.SendFriendRequest(Token, OtherFriendCode, _now);

        Assert.Equal(FriendRequestOutcome.RequestSent, outcome);
    }

    [Fact]
    public void SendFriendRequest_NewPair_StoresRequest()
    {
        AddPlayers();

        _friendRequestService.SendFriendRequest(Token, OtherFriendCode, _now);

        FriendRequest? request = _unitOfWork.FriendRequestRepository.FindBetween(AccountId, OtherAccountId);
        Assert.NotNull(request);
    }

    [Fact]
    public void SendFriendRequest_CrossRequest_ReturnsFriendshipStarted()
    {
        AddPlayers();
        AddReceivedRequest(_yesterday);

        FriendRequestOutcome outcome = _friendRequestService.SendFriendRequest(Token, OtherFriendCode, _now);

        Assert.Equal(FriendRequestOutcome.FriendshipStarted, outcome);
    }

    [Fact]
    public void SendFriendRequest_CrossRequest_StoresFriendship()
    {
        AddPlayers();
        AddReceivedRequest(_yesterday);

        _friendRequestService.SendFriendRequest(Token, OtherFriendCode, _now);

        Assert.Single(_unitOfWork.FriendshipRepository.AddedFriendships);
    }

    [Fact]
    public void SendFriendRequest_CrossRequest_RemovesRequest()
    {
        AddPlayers();
        AddReceivedRequest(_yesterday);

        _friendRequestService.SendFriendRequest(Token, OtherFriendCode, _now);

        FriendRequest? request = _unitOfWork.FriendRequestRepository.FindBetween(AccountId, OtherAccountId);
        Assert.Null(request);
    }

    [Fact]
    public void SendFriendRequest_ExpiredOwnRequest_ReplacesRequest()
    {
        AddPlayers();
        AddSentRequest(_expiredAt);

        _friendRequestService.SendFriendRequest(Token, OtherFriendCode, _now);

        FriendRequest? request = _unitOfWork.FriendRequestRepository.FindBetween(AccountId, OtherAccountId);
        Assert.Equal(_now, request?.RequestedAt);
    }

    [Fact]
    public void SendFriendRequest_NewPair_NotifiesRecipient()
    {
        AddPlayers();
        int[] expectedRecipients = { OtherAccountId };

        _friendRequestService.SendFriendRequest(Token, OtherFriendCode, _now);

        Assert.Equal(expectedRecipients, _friendNotifier.RequestRecipientAccountIds);
    }

    [Fact]
    public void SendFriendRequest_CrossRequest_NotifiesRequester()
    {
        AddPlayers();
        AddReceivedRequest(_yesterday);
        int[] expectedRecipients = { OtherAccountId };

        _friendRequestService.SendFriendRequest(Token, OtherFriendCode, _now);

        Assert.Equal(expectedRecipients, _friendNotifier.FriendshipStartedAccountIds);
    }

    [Fact]
    public void ListFriendRequests_ReceivedRequest_ListsReceived()
    {
        AddPlayers();
        AddReceivedRequest(_yesterday);

        FriendRequestBoard board = _friendRequestService.ListFriendRequests(Token, _now);

        Assert.Single(board.ReceivedRequests);
    }

    [Fact]
    public void ListFriendRequests_SentRequest_ReturnsExpiry()
    {
        AddPlayers();
        AddSentRequest(_now);
        DateTime expectedExpiry = _now.AddDays(FriendshipPolicy.RequestLifetimeDays);

        FriendRequestBoard board = _friendRequestService.ListFriendRequests(Token, _now);

        FriendRequestSummary sentRequest = board.SentRequests[0];
        Assert.Equal(expectedExpiry, sentRequest.ExpiresAt);
    }

    [Fact]
    public void ListFriendRequests_ExpiredRequest_IsLeftOut()
    {
        AddPlayers();
        AddReceivedRequest(_expiredAt);

        FriendRequestBoard board = _friendRequestService.ListFriendRequests(Token, _now);

        Assert.Empty(board.ReceivedRequests);
    }

    [Fact]
    public void AcceptFriendRequest_NoRequest_RejectsRequestNotFound()
    {
        AddPlayers();
        Action accept = () => _friendRequestService.AcceptFriendRequest(Token, OtherFriendCode, _now);

        Exception? exception = Record.Exception(accept);

        var rejection = exception as FriendshipRejectedException;
        Assert.Equal(FriendshipRejection.RequestNotFound, rejection?.Rejection);
    }

    [Fact]
    public void AcceptFriendRequest_OwnSentRequest_RejectsRequestNotFound()
    {
        AddPlayers();
        AddSentRequest(_yesterday);
        Action accept = () => _friendRequestService.AcceptFriendRequest(Token, OtherFriendCode, _now);

        Exception? exception = Record.Exception(accept);

        var rejection = exception as FriendshipRejectedException;
        Assert.Equal(FriendshipRejection.RequestNotFound, rejection?.Rejection);
    }

    [Fact]
    public void AcceptFriendRequest_ExpiredRequest_RejectsRequestNotFound()
    {
        AddPlayers();
        AddReceivedRequest(_expiredAt);
        Action accept = () => _friendRequestService.AcceptFriendRequest(Token, OtherFriendCode, _now);

        Exception? exception = Record.Exception(accept);

        var rejection = exception as FriendshipRejectedException;
        Assert.Equal(FriendshipRejection.RequestNotFound, rejection?.Rejection);
    }

    [Fact]
    public void AcceptFriendRequest_CallerAtLimit_KeepsRequest()
    {
        AddPlayers();
        AddReceivedRequest(_yesterday);
        AddFriendsUpToLimit(AccountId);
        Action accept = () => _friendRequestService.AcceptFriendRequest(Token, OtherFriendCode, _now);

        Record.Exception(accept);

        FriendRequest? request = _unitOfWork.FriendRequestRepository.FindBetween(AccountId, OtherAccountId);
        Assert.NotNull(request);
    }

    [Fact]
    public void AcceptFriendRequest_PendingRequest_StoresFriendship()
    {
        AddPlayers();
        AddReceivedRequest(_yesterday);

        _friendRequestService.AcceptFriendRequest(Token, OtherFriendCode, _now);

        Assert.Single(_unitOfWork.FriendshipRepository.AddedFriendships);
    }

    [Fact]
    public void AcceptFriendRequest_PendingRequest_RemovesRequest()
    {
        AddPlayers();
        AddReceivedRequest(_yesterday);

        _friendRequestService.AcceptFriendRequest(Token, OtherFriendCode, _now);

        FriendRequest? request = _unitOfWork.FriendRequestRepository.FindBetween(AccountId, OtherAccountId);
        Assert.Null(request);
    }

    [Fact]
    public void AcceptFriendRequest_PendingRequest_NotifiesRequester()
    {
        AddPlayers();
        AddReceivedRequest(_yesterday);
        int[] expectedRecipients = { OtherAccountId };

        _friendRequestService.AcceptFriendRequest(Token, OtherFriendCode, _now);

        Assert.Equal(expectedRecipients, _friendNotifier.FriendshipStartedAccountIds);
    }

    [Fact]
    public void RejectFriendRequest_PendingRequest_RemovesRequest()
    {
        AddPlayers();
        AddReceivedRequest(_yesterday);

        _friendRequestService.RejectFriendRequest(Token, OtherFriendCode, _now);

        FriendRequest? request = _unitOfWork.FriendRequestRepository.FindBetween(AccountId, OtherAccountId);
        Assert.Null(request);
    }

    [Fact]
    public void RejectFriendRequest_PendingRequest_SendsNoNotification()
    {
        AddPlayers();
        AddReceivedRequest(_yesterday);

        _friendRequestService.RejectFriendRequest(Token, OtherFriendCode, _now);

        Assert.Empty(_friendNotifier.FriendshipStartedAccountIds);
    }

    [Fact]
    public void CancelFriendRequest_ReceivedRequest_RejectsRequestNotFound()
    {
        AddPlayers();
        AddReceivedRequest(_yesterday);
        Action cancel = () => _friendRequestService.CancelFriendRequest(Token, OtherFriendCode, _now);

        Exception? exception = Record.Exception(cancel);

        var rejection = exception as FriendshipRejectedException;
        Assert.Equal(FriendshipRejection.RequestNotFound, rejection?.Rejection);
    }

    [Fact]
    public void CancelFriendRequest_SentRequest_RemovesRequest()
    {
        AddPlayers();
        AddSentRequest(_yesterday);

        _friendRequestService.CancelFriendRequest(Token, OtherFriendCode, _now);

        FriendRequest? request = _unitOfWork.FriendRequestRepository.FindBetween(AccountId, OtherAccountId);
        Assert.Null(request);
    }

    private void AddPlayers()
    {
        _sessionVerifier.Add(Token, _caller);
        _unitOfWork.AccountRepository.AddPlayerIdentity(_caller);
        _unitOfWork.AccountRepository.AddPlayerIdentity(_otherPlayer);
    }

    private void AddFriendship()
    {
        var friendship = new Friendship(AccountId, OtherAccountId, _now);
        _unitOfWork.FriendshipRepository.Add(friendship);
    }

    private void AddSentRequest(DateTime requestedAt)
    {
        var request = new FriendRequest(AccountId, OtherAccountId, requestedAt);
        _unitOfWork.FriendRequestRepository.Add(request);
    }

    private void AddReceivedRequest(DateTime requestedAt)
    {
        var request = new FriendRequest(OtherAccountId, AccountId, requestedAt);
        _unitOfWork.FriendRequestRepository.Add(request);
    }

    private void AddFriendsUpToLimit(int accountId)
    {
        for (int index = 0; index < FriendshipPolicy.MaximumFriends; index++)
        {
            int friendAccountId = FirstFillerAccountId + index;
            string friendNickname = $"{FillerNicknamePrefix}{index}";
            var friend = new PlayerIdentity(friendAccountId, friendNickname, UnusedFriendCode);
            var summary = new FriendSummary(friend, lastSeenAt: null);
            _unitOfWork.FriendshipRepository.AddFriendSummary(accountId, summary);
        }
    }
}
