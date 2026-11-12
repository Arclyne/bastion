using System;
using System.Collections.Generic;
using Bastion.Client.Networking.Friends;
using Bastion.Contracts.Friends;
using Xunit;

namespace Bastion.Client.Tests.Friends;

public sealed class TestFriendContractTranslator
{
    private const string OwnFriendCode = "QU7K2P";
    private const string FriendNickname = "marta_muros";
    private const string FriendCode = "MA3R7T";

    private static readonly DateTime _listedAt = new DateTime(2026, 11, 5, 18, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void ToFriendListing_ConnectedEntry_MapsConnectedPresence()
    {
        FriendListEntry entry = CreateEntry(FriendCode);
        FriendList list = CreateList(entry);

        var listing = FriendContractTranslator.ToFriendListing(list);

        ListedFriend friend = listing.Friends[0];
        Assert.Equal(FriendPresence.Connected, friend.Presence);
    }

    [Fact]
    public void ToFriendListing_NullFriends_ReturnsEmptyList()
    {
        var list = new FriendList
        {
            OwnFriendCode = OwnFriendCode,
            ListedAt = _listedAt,
        };

        var listing = FriendContractTranslator.ToFriendListing(list);

        Assert.Empty(listing.Friends);
    }

    [Fact]
    public void ToFriendListing_NullFriendCode_UsesEmptyText()
    {
        FriendListEntry entry = CreateEntry(friendCode: null);
        FriendList list = CreateList(entry);

        var listing = FriendContractTranslator.ToFriendListing(list);

        ListedFriend friend = listing.Friends[0];
        Assert.Equal(string.Empty, friend.Friend.FriendCode);
    }

    [Fact]
    public void ToFriendListing_AnyList_KeepsListedAt()
    {
        FriendListEntry entry = CreateEntry(FriendCode);
        FriendList list = CreateList(entry);

        var listing = FriendContractTranslator.ToFriendListing(list);

        Assert.Equal(_listedAt, listing.ListedAt);
    }

    [Fact]
    public void ToFoundPlayers_SelfEntry_MapsOwnAccount()
    {
        var selfEntry = new PlayerSearchEntry
        {
            Nickname = FriendNickname,
            FriendCode = FriendCode,
            Relationship = PlayerRelationship.Self,
        };
        PlayerSearchEntry[] entries = { selfEntry };

        IReadOnlyList<FoundPlayer> foundPlayers = FriendContractTranslator.ToFoundPlayers(entries);

        FoundPlayer foundPlayer = foundPlayers[0];
        Assert.Equal(FriendRelationship.OwnAccount, foundPlayer.Relationship);
    }

    [Fact]
    public void ToFoundPlayers_NullEntries_ReturnsEmptyList()
    {
        IReadOnlyList<FoundPlayer> foundPlayers = FriendContractTranslator.ToFoundPlayers(entries: null);

        Assert.Empty(foundPlayers);
    }

    [Fact]
    public void ToRejection_FriendLimitReached_MapsFriendLimitReached()
    {
        FriendshipRejection rejection =
            FriendContractTranslator.ToRejection(FriendshipRejectionReason.FriendLimitReached);

        Assert.Equal(FriendshipRejection.FriendLimitReached, rejection);
    }

    [Fact]
    public void ToFriendRequestListing_NullLists_ReturnsEmptyReceived()
    {
        var list = new FriendRequestList { ListedAt = _listedAt };

        var listing = FriendContractTranslator.ToFriendRequestListing(list);

        Assert.Empty(listing.ReceivedRequests);
    }

    private static FriendListEntry CreateEntry(string? friendCode)
    {
        return new FriendListEntry
        {
            Nickname = FriendNickname,
            FriendCode = friendCode,
            Presence = PresenceState.Connected,
        };
    }

    private static FriendList CreateList(FriendListEntry entry)
    {
        FriendListEntry[] friends = { entry };
        return new FriendList
        {
            OwnFriendCode = OwnFriendCode,
            Friends = friends,
            ListedAt = _listedAt,
        };
    }
}
