using System;
using Bastion.Domain.Friends;
using Xunit;

namespace Bastion.Domain.Tests.Friends;

public class TestFriendship
{
    private const int SmallerAccountId = 3;
    private const int LargerAccountId = 4;

    private static readonly DateTime _befriendedAt = new DateTime(2026, 8, 10, 19, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Constructor_LargerIdFirst_SetsSmallerAccountId()
    {
        var friendship = new Friendship(LargerAccountId, SmallerAccountId, _befriendedAt);

        Assert.Equal(SmallerAccountId, friendship.SmallerAccountId);
    }

    [Fact]
    public void Constructor_LargerIdFirst_SetsLargerAccountId()
    {
        var friendship = new Friendship(LargerAccountId, SmallerAccountId, _befriendedAt);

        Assert.Equal(LargerAccountId, friendship.LargerAccountId);
    }

    [Fact]
    public void Constructor_SameAccount_ThrowsArgument()
    {
        Assert.Throws<ArgumentException>(() => new Friendship(SmallerAccountId, SmallerAccountId, _befriendedAt));
    }
}
