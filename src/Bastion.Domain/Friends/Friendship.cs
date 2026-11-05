using System;

namespace Bastion.Domain.Friends;

public class Friendship
{
    private const string SameAccountMessage = "A player cannot be friends with themselves.";
    private const string StrangerMessage = "The account does not belong to this friendship.";

    public Friendship(int firstAccountId, int secondAccountId, DateTime befriendedAt)
    {
        if (firstAccountId == secondAccountId)
        {
            throw new ArgumentException(SameAccountMessage, nameof(secondAccountId));
        }

        AccountAId = Math.Min(firstAccountId, secondAccountId);
        AccountBId = Math.Max(firstAccountId, secondAccountId);
        BefriendedAt = befriendedAt;
    }

    private Friendship()
    {
    }

    public int AccountAId { get; private set; }

    public int AccountBId { get; private set; }

    public DateTime BefriendedAt { get; private set; }

    public int GetFriendAccountId(int accountId)
    {
        if (accountId == AccountAId)
        {
            return AccountBId;
        }

        if (accountId == AccountBId)
        {
            return AccountAId;
        }

        throw new ArgumentException(StrangerMessage, nameof(accountId));
    }
}
