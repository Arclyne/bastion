using System;

namespace Bastion.Domain.Friends;

public class Friendship
{
    private const string SameAccountMessage = "A player cannot be friends with themselves.";

    public Friendship(int firstAccountId, int secondAccountId, DateTime befriendedAt)
    {
        if (firstAccountId == secondAccountId)
        {
            throw new ArgumentException(SameAccountMessage, nameof(secondAccountId));
        }

        SmallerAccountId = Math.Min(firstAccountId, secondAccountId);
        LargerAccountId = Math.Max(firstAccountId, secondAccountId);
        BefriendedAt = befriendedAt;
    }

    private Friendship()
    {
    }

    public int SmallerAccountId { get; private set; }

    public int LargerAccountId { get; private set; }

    public DateTime BefriendedAt { get; private set; }
}
