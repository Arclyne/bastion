using System;
using Bastion.Domain.Friends;
using Xunit;

namespace Bastion.Domain.Tests.Friends;

public class TestFriendRequest
{
    private const int RequesterId = 3;
    private const int RecipientId = 4;

    private static readonly DateTime _requestedAt = new DateTime(2026, 11, 1, 18, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void IsExpiredAt_ThirtyDaysLater_ReturnsTrue()
    {
        var request = new FriendRequest(RequesterId, RecipientId, _requestedAt);

        bool isExpired = request.IsExpiredAt(_requestedAt.AddDays(FriendshipRules.RequestLifetimeDays));

        Assert.True(isExpired);
    }

    [Fact]
    public void IsExpiredAt_TwentyNineDaysLater_ReturnsFalse()
    {
        var request = new FriendRequest(RequesterId, RecipientId, _requestedAt);

        bool isExpired = request.IsExpiredAt(_requestedAt.AddDays(FriendshipRules.RequestLifetimeDays - 1));

        Assert.False(isExpired);
    }

    [Fact]
    public void Constructor_SameAccount_ThrowsArgument()
    {
        Assert.Throws<ArgumentException>(() => new FriendRequest(RequesterId, RequesterId, _requestedAt));
    }

    [Fact]
    public void IsSentBy_Requester_ReturnsTrue()
    {
        var request = new FriendRequest(RequesterId, RecipientId, _requestedAt);

        bool isSentBy = request.IsSentBy(RequesterId);

        Assert.True(isSentBy);
    }
}
