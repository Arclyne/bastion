using System;
using Bastion.Domain.Friends;
using Xunit;

namespace Bastion.Domain.Tests.Friends;

public class TestFriendship
{
    private const int SmallerAccountId = 3;
    private const int LargerAccountId = 4;
    private const int StrangerAccountId = 9;

    private static readonly DateTime _befriendedAt = new DateTime(2026, 8, 10, 19, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Constructor_LargerIdFirst_StoresSmallerAsAccountA()
    {
        var friendship = new Friendship(LargerAccountId, SmallerAccountId, _befriendedAt);

        Assert.Equal(SmallerAccountId, friendship.AccountAId);
    }

    [Fact]
    public void Constructor_LargerIdFirst_StoresLargerAsAccountB()
    {
        var friendship = new Friendship(LargerAccountId, SmallerAccountId, _befriendedAt);

        Assert.Equal(LargerAccountId, friendship.AccountBId);
    }

    [Fact]
    public void Constructor_SameAccount_ThrowsArgument()
    {
        Assert.Throws<ArgumentException>(() => new Friendship(SmallerAccountId, SmallerAccountId, _befriendedAt));
    }

    [Fact]
    public void GetFriendAccountId_AccountA_ReturnsAccountB()
    {
        var friendship = new Friendship(SmallerAccountId, LargerAccountId, _befriendedAt);

        int friendAccountId = friendship.GetFriendAccountId(SmallerAccountId);

        Assert.Equal(LargerAccountId, friendAccountId);
    }

    [Fact]
    public void GetFriendAccountId_AccountB_ReturnsAccountA()
    {
        var friendship = new Friendship(SmallerAccountId, LargerAccountId, _befriendedAt);

        int friendAccountId = friendship.GetFriendAccountId(LargerAccountId);

        Assert.Equal(SmallerAccountId, friendAccountId);
    }

    [Fact]
    public void GetFriendAccountId_Stranger_ThrowsArgument()
    {
        var friendship = new Friendship(SmallerAccountId, LargerAccountId, _befriendedAt);

        Assert.Throws<ArgumentException>(() => friendship.GetFriendAccountId(StrangerAccountId));
    }
}
