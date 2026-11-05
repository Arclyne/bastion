using System;

namespace Bastion.Domain.Friends;

public class FriendRequest
{
    private const string SameAccountMessage = "A player cannot send a friend request to themselves.";

    public FriendRequest(int requesterId, int recipientId, DateTime requestedAt)
    {
        if (requesterId == recipientId)
        {
            throw new ArgumentException(SameAccountMessage, nameof(recipientId));
        }

        RequesterId = requesterId;
        RecipientId = recipientId;
        RequestedAt = requestedAt;
    }

    private FriendRequest()
    {
    }

    public int RequesterId { get; private set; }

    public int RecipientId { get; private set; }

    public DateTime RequestedAt { get; private set; }

    public DateTime ExpiresAt => RequestedAt.AddDays(FriendshipRules.RequestLifetimeDays);

    public bool IsExpiredAt(DateTime now)
    {
        return now >= ExpiresAt;
    }

    public bool IsSentBy(int accountId)
    {
        return RequesterId == accountId;
    }
}
