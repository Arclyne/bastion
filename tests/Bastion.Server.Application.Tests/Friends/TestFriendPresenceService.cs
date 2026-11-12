using System;
using Bastion.Domain.Accounts;
using Bastion.Server.Application.Accounts;
using Bastion.Server.Application.Friends;
using Bastion.Server.Application.Tests.Fakes;
using Xunit;

namespace Bastion.Server.Application.Tests.Friends;

public sealed class TestFriendPresenceService
{
    private const int AccountId = 3;
    private const int UnsavedAccountId = 0;
    private const string Nickname = "luis_quo";
    private const string FriendCode = "QU7K2P";
    private const string Email = "luis.quo@correo.mx";
    private const int FriendAccountId = 4;
    private const string FriendNickname = "marta_muros";
    private const string FriendFriendCode = "MA3R7T";
    private const int SecondFriendAccountId = 5;
    private const int UnknownAccountId = 99;
    private const int OneSave = 1;

    private static readonly DateTime _now = new DateTime(2026, 11, 5, 18, 0, 0, DateTimeKind.Utc);
    private static readonly PlayerIdentity _identity = new PlayerIdentity(AccountId, Nickname, FriendCode);

    private readonly FakeUnitOfWork _unitOfWork = new FakeUnitOfWork();
    private readonly FakeFriendNotifier _friendNotifier = new FakeFriendNotifier();
    private readonly FriendPresenceService _friendPresenceService;

    public TestFriendPresenceService()
    {
        _friendPresenceService = new FriendPresenceService(_unitOfWork, _friendNotifier);
    }

    [Fact]
    public void ReportConnected_TwoFriends_NotifiesBothFriends()
    {
        _unitOfWork.AccountRepository.AddPlayerIdentity(_identity);
        AddFriend(AccountId, FriendAccountId);
        AddFriend(AccountId, SecondFriendAccountId);
        int[] expectedRecipients = { FriendAccountId, SecondFriendAccountId };

        _friendPresenceService.ReportConnected(AccountId);

        Assert.Equal(expectedRecipients, _friendNotifier.RecipientAccountIds);
    }

    [Fact]
    public void ReportConnected_AnyFriend_SendsConnectedPresence()
    {
        _unitOfWork.AccountRepository.AddPlayerIdentity(_identity);
        AddFriend(AccountId, FriendAccountId);

        _friendPresenceService.ReportConnected(AccountId);

        FriendPresenceChange change = _friendNotifier.Changes[0];
        Assert.Equal(FriendPresence.Connected, change.Presence);
    }

    [Fact]
    public void ReportConnected_NoFriends_SendsNoNotification()
    {
        _unitOfWork.AccountRepository.AddPlayerIdentity(_identity);

        _friendPresenceService.ReportConnected(AccountId);

        Assert.Empty(_friendNotifier.Changes);
    }

    [Fact]
    public void ReportConnected_UnknownAccount_SendsNoNotification()
    {
        AddFriend(UnknownAccountId, FriendAccountId);

        _friendPresenceService.ReportConnected(UnknownAccountId);

        Assert.Empty(_friendNotifier.Changes);
    }

    [Fact]
    public void ReportDisconnected_StoredAccount_RecordsLastSeen()
    {
        Account account = AddAccount();

        _friendPresenceService.ReportDisconnected(UnsavedAccountId, _now);

        Assert.Equal(_now, account.LastSeenAt);
    }

    [Fact]
    public void ReportDisconnected_StoredAccount_SavesOnce()
    {
        AddAccount();

        _friendPresenceService.ReportDisconnected(UnsavedAccountId, _now);

        Assert.Equal(OneSave, _unitOfWork.SaveCount);
    }

    [Fact]
    public void ReportDisconnected_AnyFriend_SendsDisconnectedPresence()
    {
        AddAccount();
        AddFriend(UnsavedAccountId, FriendAccountId);

        _friendPresenceService.ReportDisconnected(UnsavedAccountId, _now);

        FriendPresenceChange change = _friendNotifier.Changes[0];
        Assert.Equal(FriendPresence.Disconnected, change.Presence);
    }

    [Fact]
    public void ReportDisconnected_AnyFriend_SendsLastSeen()
    {
        AddAccount();
        AddFriend(UnsavedAccountId, FriendAccountId);

        _friendPresenceService.ReportDisconnected(UnsavedAccountId, _now);

        FriendPresenceChange change = _friendNotifier.Changes[0];
        Assert.Equal(_now, change.LastSeenAt);
    }

    private void AddFriend(int accountId, int friendAccountId)
    {
        var friend = new PlayerIdentity(friendAccountId, FriendNickname, FriendFriendCode);
        var summary = new FriendSummary(friend, lastSeenAt: null);
        _unitOfWork.FriendshipRepository.AddFriendSummary(accountId, summary);
    }

    private Account AddAccount()
    {
        var account = new Account(Nickname, Email);
        _unitOfWork.AccountRepository.AddAccount(account);
        return account;
    }
}
