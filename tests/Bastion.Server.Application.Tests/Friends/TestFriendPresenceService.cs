using System;
using Bastion.Domain.Accounts;
using Bastion.Server.Application.Accounts;
using Bastion.Server.Application.Friends;
using Bastion.Server.Application.Tests.Fakes;
using Xunit;

namespace Bastion.Server.Application.Tests.Friends;

public class TestFriendPresenceService
{
    private const int AccountId = 3;
    private const int StoredAccountId = 0;
    private const string Nickname = "luis_quo";
    private const string FriendCode = "QU7K2P";
    private const string Email = "luis.quo@correo.mx";
    private const int FriendAccountId = 4;
    private const int SecondFriendAccountId = 5;
    private const int UnknownAccountId = 99;

    private static readonly DateTime _now = new DateTime(2026, 11, 5, 18, 0, 0, DateTimeKind.Utc);

    private readonly FakeUnitOfWork _unitOfWork = new FakeUnitOfWork();
    private readonly FakeFriendNotifier _friendNotifier = new FakeFriendNotifier();
    private readonly FriendPresenceService _friendPresenceService;

    public TestFriendPresenceService()
    {
        _friendPresenceService = new FriendPresenceService(_unitOfWork, _friendNotifier);
        _unitOfWork.AccountStore.StorePlayerIdentity(new PlayerIdentity(AccountId, Nickname, FriendCode));
    }

    [Fact]
    public void ReportConnected_TwoFriends_NotifiesBothFriends()
    {
        StoreFriend(AccountId, FriendAccountId);
        StoreFriend(AccountId, SecondFriendAccountId);

        _friendPresenceService.ReportConnected(AccountId);

        Assert.Equal(new[] { FriendAccountId, SecondFriendAccountId }, _friendNotifier.RecipientAccountIds);
    }

    [Fact]
    public void ReportConnected_AnyFriend_SendsConnectedPresence()
    {
        StoreFriend(AccountId, FriendAccountId);

        _friendPresenceService.ReportConnected(AccountId);

        Assert.Equal(FriendPresence.Connected, _friendNotifier.Changes[0].Presence);
    }

    [Fact]
    public void ReportConnected_NoFriends_SendsNoNotification()
    {
        _friendPresenceService.ReportConnected(AccountId);

        Assert.Empty(_friendNotifier.Changes);
    }

    [Fact]
    public void ReportConnected_UnknownAccount_SendsNoNotification()
    {
        StoreFriend(UnknownAccountId, FriendAccountId);

        _friendPresenceService.ReportConnected(UnknownAccountId);

        Assert.Empty(_friendNotifier.Changes);
    }

    [Fact]
    public void ReportDisconnected_StoredAccount_RecordsLastSeen()
    {
        Account account = StoreAccount();

        _friendPresenceService.ReportDisconnected(StoredAccountId, _now);

        Assert.Equal(_now, account.LastSeenAt);
    }

    [Fact]
    public void ReportDisconnected_StoredAccount_SavesOnce()
    {
        StoreAccount();

        _friendPresenceService.ReportDisconnected(StoredAccountId, _now);

        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public void ReportDisconnected_AnyFriend_SendsDisconnectedPresence()
    {
        StoreAccount();
        StoreFriend(StoredAccountId, FriendAccountId);

        _friendPresenceService.ReportDisconnected(StoredAccountId, _now);

        Assert.Equal(FriendPresence.Disconnected, _friendNotifier.Changes[0].Presence);
    }

    [Fact]
    public void ReportDisconnected_AnyFriend_SendsLastSeen()
    {
        StoreAccount();
        StoreFriend(StoredAccountId, FriendAccountId);

        _friendPresenceService.ReportDisconnected(StoredAccountId, _now);

        Assert.Equal(_now, _friendNotifier.Changes[0].LastSeenAt);
    }

    private Account StoreAccount()
    {
        var account = new Account(Nickname, Email);
        _unitOfWork.AccountStore.Store(account);
        return account;
    }

    private void StoreFriend(int accountId, int friendAccountId)
    {
        var friend = new PlayerIdentity(friendAccountId, "marta_muros", "MA3R7T");
        _unitOfWork.FriendshipStore.Store(accountId, new FriendSummary(friend, null));
    }
}
