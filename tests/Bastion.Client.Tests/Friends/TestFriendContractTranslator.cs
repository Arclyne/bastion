using System;
using Bastion.Client.Networking.Friends;
using Bastion.Contracts.Friends;
using Xunit;

namespace Bastion.Client.Tests.Friends;

public class TestFriendContractTranslator
{
    private const string OwnFriendCode = "QU7K2P";
    private const string FriendNickname = "marta_muros";
    private const string FriendCode = "MA3R7T";

    private static readonly DateTime _listedAt = new DateTime(2026, 11, 5, 18, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void ToFriendListing_ConnectedEntry_MapsConnectedPresence()
    {
        FriendList list = CreateList(CreateEntry(FriendCode));

        var listing = FriendContractTranslator.ToFriendListing(list);

        Assert.Equal(FriendPresence.Connected, listing.Friends[0].Presence);
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
        FriendList list = CreateList(CreateEntry(null));

        var listing = FriendContractTranslator.ToFriendListing(list);

        Assert.Equal(string.Empty, listing.Friends[0].Friend.FriendCode);
    }

    [Fact]
    public void ToFriendListing_AnyList_KeepsListedAt()
    {
        FriendList list = CreateList(CreateEntry(FriendCode));

        var listing = FriendContractTranslator.ToFriendListing(list);

        Assert.Equal(_listedAt, listing.ListedAt);
    }

    private static FriendList CreateList(FriendListEntry entry)
    {
        return new FriendList
        {
            OwnFriendCode = OwnFriendCode,
            Friends = new[] { entry },
            ListedAt = _listedAt,
        };
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
}
