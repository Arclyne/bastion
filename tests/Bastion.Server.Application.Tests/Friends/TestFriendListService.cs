using System;
using Bastion.Server.Application.Friends;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Application.Tests.Fakes;
using Xunit;

namespace Bastion.Server.Application.Tests.Friends;

public class TestFriendListService
{
    private const int AccountId = 3;
    private const string Nickname = "luis_quo";
    private const string FriendCode = "QU7K2P";
    private const string Token = "luis-token";
    private const string UnknownToken = "unknown-token";
    private const int FriendAccountId = 4;
    private const int SecondFriendAccountId = 5;
    private const int ThirdFriendAccountId = 6;

    private static readonly DateTime _now = new DateTime(2026, 11, 5, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _lastSeenAt = new DateTime(2026, 11, 5, 16, 0, 0, DateTimeKind.Utc);

    private readonly FakeUnitOfWork _unitOfWork = new FakeUnitOfWork();
    private readonly FakeSessionVerifier _sessionVerifier = new FakeSessionVerifier();
    private readonly FakePlayerPresence _playerPresence = new FakePlayerPresence();
    private readonly FriendListService _friendListService;

    public TestFriendListService()
    {
        _friendListService = new FriendListService(_unitOfWork, _sessionVerifier, _playerPresence);
        _sessionVerifier.Add(Token, new ConnectedPlayer(AccountId, Nickname));
        _unitOfWork.AccountStore.StorePlayerIdentity(new PlayerIdentity(AccountId, Nickname, FriendCode));
    }

    [Fact]
    public void ListFriends_InvalidSession_ThrowsInvalidSession()
    {
        Assert.Throws<InvalidSessionException>(() => _friendListService.ListFriends(UnknownToken, _now));
    }

    [Fact]
    public void ListFriends_UnknownCallerAccount_ThrowsInvalidSession()
    {
        _sessionVerifier.Add(UnknownToken, new ConnectedPlayer(ThirdFriendAccountId, "sofia_salto"));

        Assert.Throws<InvalidSessionException>(() => _friendListService.ListFriends(UnknownToken, _now));
    }

    [Fact]
    public void ListFriends_NoFriends_ReturnsEmptyList()
    {
        FriendRoster roster = _friendListService.ListFriends(Token, _now);

        Assert.Empty(roster.Friends);
    }

    [Fact]
    public void ListFriends_ConnectedFriend_ReturnsConnectedPresence()
    {
        StoreFriend(FriendAccountId, "marta_muros");
        _playerPresence.ConnectedAccountIds.Add(FriendAccountId);

        FriendRoster roster = _friendListService.ListFriends(Token, _now);

        Assert.Equal(FriendPresence.Connected, roster.Friends[0].Presence);
    }

    [Fact]
    public void ListFriends_DisconnectedFriend_ReturnsDisconnectedPresence()
    {
        StoreFriend(FriendAccountId, "marta_muros");

        FriendRoster roster = _friendListService.ListFriends(Token, _now);

        Assert.Equal(FriendPresence.Disconnected, roster.Friends[0].Presence);
    }

    [Fact]
    public void ListFriends_DisconnectedFriend_ReturnsLastSeen()
    {
        StoreFriend(FriendAccountId, "marta_muros");

        FriendRoster roster = _friendListService.ListFriends(Token, _now);

        Assert.Equal(_lastSeenAt, roster.Friends[0].Summary.LastSeenAt);
    }

    [Fact]
    public void ListFriends_AnyCaller_ReturnsOwnFriendCode()
    {
        FriendRoster roster = _friendListService.ListFriends(Token, _now);

        Assert.Equal(FriendCode, roster.OwnFriendCode);
    }

    [Fact]
    public void ListFriends_AnyCaller_ReturnsListedAtNow()
    {
        FriendRoster roster = _friendListService.ListFriends(Token, _now);

        Assert.Equal(_now, roster.ListedAt);
    }

    [Fact]
    public void ListFriends_UnsortedFriends_ReturnsAlphabeticalFirst()
    {
        StoreFriend(ThirdFriendAccountId, "sofia_salto");
        StoreFriend(SecondFriendAccountId, "Pedro_peon");
        StoreFriend(FriendAccountId, "marta_muros");

        FriendRoster roster = _friendListService.ListFriends(Token, _now);

        Assert.Equal("marta_muros", roster.Friends[0].Summary.Friend.Nickname);
    }

    private void StoreFriend(int friendAccountId, string friendNickname)
    {
        var friend = new PlayerIdentity(friendAccountId, friendNickname, "AB2CD9");
        _unitOfWork.FriendshipStore.Store(AccountId, new FriendSummary(friend, _lastSeenAt));
    }
}
