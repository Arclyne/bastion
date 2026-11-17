using System;
using Bastion.Domain.Friends;
using Xunit;

namespace Bastion.Domain.Tests.Friends;

public sealed class TestFriendRequest
{
    private const int RequesterId = 3;
    private const int RecipientId = 4;

    private static readonly DateTime _requestedAt = new DateTime(2026, 11, 1, 18, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void IsExpiredAt_ThirtyDaysLater_ReturnsTrue()
    {
        FriendRequest request = CreateRequest();
        DateTime expiry = _requestedAt.AddDays(FriendshipPolicy.RequestLifetimeDays);

        bool isExpired = request.IsExpiredAt(expiry);

        Assert.True(isExpired);
    }

    [Fact]
    public void IsExpiredAt_TwentyNineDaysLater_ReturnsFalse()
    {
        FriendRequest request = CreateRequest();
        int daysBeforeExpiry = FriendshipPolicy.RequestLifetimeDays - 1;
        DateTime dayBeforeExpiry = _requestedAt.AddDays(daysBeforeExpiry);

        bool isExpired = request.IsExpiredAt(dayBeforeExpiry);

        Assert.False(isExpired);
    }

    [Fact]
    public void Constructor_SameAccount_ThrowsArgumentException()
    {
        Action requestSelf = () => new FriendRequest(RequesterId, RequesterId, _requestedAt);

        Assert.Throws<ArgumentException>(requestSelf);
    }

    [Fact]
    public void IsSentBy_Requester_ReturnsTrue()
    {
        FriendRequest request = CreateRequest();

        bool isSentBy = request.IsSentBy(RequesterId);

        Assert.True(isSentBy);
    }

    private static FriendRequest CreateRequest()
    {
        return new FriendRequest(RequesterId, RecipientId, _requestedAt);
    }
}
