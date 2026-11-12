using System;
using Bastion.Server.Application.Accounts;
using Bastion.Server.Application.Friends;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Application.Tests.Fakes;
using Xunit;

namespace Bastion.Server.Application.Tests.Friends;

public sealed class TestFriendListService
{
    private const int AccountId = 3;
    private const string Nickname = "luis_quo";
    private const string FriendCode = "QU7K2P";
    private const string Token = "luis-token";
    private const string UnknownToken = "unknown-token";
    private const int FriendAccountId = 4;
    private const string FriendNickname = "marta_muros";
    private const int SecondFriendAccountId = 5;
    private const string SecondFriendNickname = "Pedro_peon";
    private const int ThirdFriendAccountId = 6;
    private const string ThirdFriendNickname = "sofia_salto";
    private const string StoredFriendCode = "AB2CD9";
    private const string HyphenatedStoredFriendCode = "ab-2cd9";

    private static readonly DateTime _now = new DateTime(2026, 11, 5, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _lastSeenAt = new DateTime(2026, 11, 5, 16, 0, 0, DateTimeKind.Utc);
    private static readonly PlayerIdentity _caller = new PlayerIdentity(AccountId, Nickname, FriendCode);
    private static readonly ConnectedPlayer _unknownCaller = new ConnectedPlayer(ThirdFriendAccountId, ThirdFriendNickname);

    private readonly FakeUnitOfWork _unitOfWork = new FakeUnitOfWork();
    private readonly FakeSessionVerifier _sessionVerifier = new FakeSessionVerifier();
    private readonly FakePlayerPresence _playerPresence = new FakePlayerPresence();
    private readonly FriendListService _friendListService;

    public TestFriendListService()
    {
        _friendListService = new FriendListService(_unitOfWork, _sessionVerifier, _playerPresence);
    }

    [Fact]
    public void ListFriends_InvalidSession_ThrowsInvalidSession()
    {
        Action list = () => _friendListService.ListFriends(UnknownToken, _now);

        Assert.Throws<InvalidSessionException>(list);
    }

    [Fact]
    public void ListFriends_UnknownCallerAccount_ThrowsInvalidSession()
    {
        _sessionVerifier.Add(UnknownToken, _unknownCaller);

        Action list = () => _friendListService.ListFriends(UnknownToken, _now);

        Assert.Throws<InvalidSessionException>(list);
    }

    [Fact]
    public void ListFriends_NoFriends_ReturnsEmptyList()
    {
        AddCaller();

        FriendRoster roster = _friendListService.ListFriends(Token, _now);

        Assert.Empty(roster.Friends);
    }

    [Fact]
    public void ListFriends_ConnectedFriend_ReturnsConnectedPresence()
    {
        AddCaller();
        AddFriend(FriendAccountId, FriendNickname);
        _playerPresence.ConnectedAccountIds.Add(FriendAccountId);

        FriendRoster roster = _friendListService.ListFriends(Token, _now);

        FriendStatus friend = roster.Friends[0];
        Assert.Equal(FriendPresence.Connected, friend.Presence);
    }

    [Fact]
    public void ListFriends_DisconnectedFriend_ReturnsDisconnectedPresence()
    {
        AddCaller();
        AddFriend(FriendAccountId, FriendNickname);

        FriendRoster roster = _friendListService.ListFriends(Token, _now);

        FriendStatus friend = roster.Friends[0];
        Assert.Equal(FriendPresence.Disconnected, friend.Presence);
    }

    [Fact]
    public void ListFriends_DisconnectedFriend_ReturnsLastSeen()
    {
        AddCaller();
        AddFriend(FriendAccountId, FriendNickname);

        FriendRoster roster = _friendListService.ListFriends(Token, _now);

        FriendStatus friend = roster.Friends[0];
        Assert.Equal(_lastSeenAt, friend.Summary.LastSeenAt);
    }

    [Fact]
    public void ListFriends_AnyCaller_ReturnsOwnFriendCode()
    {
        AddCaller();

        FriendRoster roster = _friendListService.ListFriends(Token, _now);

        Assert.Equal(FriendCode, roster.OwnFriendCode);
    }

    [Fact]
    public void ListFriends_AnyCaller_ReturnsListedAtNow()
    {
        AddCaller();

        FriendRoster roster = _friendListService.ListFriends(Token, _now);

        Assert.Equal(_now, roster.ListedAt);
    }

    [Fact]
    public void ListFriends_UnsortedFriends_ReturnsAlphabeticalFirst()
    {
        AddCaller();
        AddFriend(ThirdFriendAccountId, ThirdFriendNickname);
        AddFriend(SecondFriendAccountId, SecondFriendNickname);
        AddFriend(FriendAccountId, FriendNickname);

        FriendRoster roster = _friendListService.ListFriends(Token, _now);

        FriendStatus first = roster.Friends[0];
        Assert.Equal(FriendNickname, first.Summary.Friend.Nickname);
    }

    [Fact]
    public void RemoveFriend_InvalidSession_ThrowsInvalidSession()
    {
        Action remove = () => _friendListService.RemoveFriend(UnknownToken, StoredFriendCode, _now);

        Assert.Throws<InvalidSessionException>(remove);
    }

    [Fact]
    public void RemoveFriend_Stranger_RejectsNotFriends()
    {
        AddCaller();
        Action remove = () => _friendListService.RemoveFriend(Token, StoredFriendCode, _now);

        Exception? exception = Record.Exception(remove);

        var rejection = exception as FriendshipRejectedException;
        Assert.Equal(FriendshipRejection.NotFriends, rejection?.Rejection);
    }

    [Fact]
    public void RemoveFriend_HyphenatedCode_RemovesFriendship()
    {
        AddCaller();
        AddFriend(FriendAccountId, FriendNickname);
        int[] expectedRemovedAccountIds = { FriendAccountId };

        _friendListService.RemoveFriend(Token, HyphenatedStoredFriendCode, _now);

        Assert.Equal(expectedRemovedAccountIds, _unitOfWork.FriendshipRepository.RemovedFriendAccountIds);
    }

    private void AddCaller()
    {
        _sessionVerifier.Add(Token, _caller);
        _unitOfWork.AccountRepository.AddPlayerIdentity(_caller);
    }

    private void AddFriend(int friendAccountId, string friendNickname)
    {
        var friend = new PlayerIdentity(friendAccountId, friendNickname, StoredFriendCode);
        var summary = new FriendSummary(friend, _lastSeenAt);
        _unitOfWork.FriendshipRepository.AddFriendSummary(AccountId, summary);
    }
}
