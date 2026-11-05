using System;
using System.Collections.Generic;
using Bastion.Domain.Friends;
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
    private readonly FriendRequestService _friendRequestService;

    public TestFriendRequestService()
    {
        _friendRequestService = new FriendRequestService(_unitOfWork, _sessionVerifier);
        _sessionVerifier.Add(Token, new ConnectedPlayer(AccountId, Nickname));
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
        DateTime expiredAt = _now.AddDays(-FriendshipRules.RequestLifetimeDays);
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
        DateTime expiredAt = _now.AddDays(-FriendshipRules.RequestLifetimeDays);
        _unitOfWork.FriendRequestStore.Add(new FriendRequest(AccountId, OtherAccountId, expiredAt));

        _friendRequestService.SendFriendRequest(Token, OtherFriendCode, _now);

        Assert.Equal(_now, _unitOfWork.FriendRequestStore.FindBetween(AccountId, OtherAccountId)?.RequestedAt);
    }

    private FriendshipRejectedException? RecordRejection(string friendCode)
    {
        return Record.Exception(() => _friendRequestService.SendFriendRequest(Token, friendCode, _now))
            as FriendshipRejectedException;
    }

    private void StoreFriendsUpToLimit(int accountId)
    {
        for (int index = 0; index < FriendshipRules.MaximumFriends; index++)
        {
            var friend = new PlayerIdentity(FirstFillerAccountId + index, $"amigo_{index}", UnusedFriendCode);
            _unitOfWork.FriendshipStore.Store(accountId, new FriendSummary(friend, null));
        }
    }
}
