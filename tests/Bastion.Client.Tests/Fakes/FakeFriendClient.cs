using System;
using System.Collections.Generic;
using Bastion.Client.Networking.Friends;

namespace Bastion.Client.Tests.Fakes;

public sealed class FakeFriendClient : IFriendClient
{
    private const string CallNotSupported = "The tests that use this fake never call the friends service.";

    public FriendListing ListFriends(string sessionToken)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        throw new NotSupportedException(CallNotSupported);
    }

    public IReadOnlyList<FoundPlayer> SearchPlayers(string sessionToken, string text)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(text);

        throw new NotSupportedException(CallNotSupported);
    }

    public FriendRequestOutcome SendFriendRequest(string sessionToken, string friendCode)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(friendCode);

        throw new NotSupportedException(CallNotSupported);
    }

    public FriendRequestListing ListFriendRequests(string sessionToken)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        throw new NotSupportedException(CallNotSupported);
    }

    public void AcceptFriendRequest(string sessionToken, string requesterFriendCode)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(requesterFriendCode);

        throw new NotSupportedException(CallNotSupported);
    }

    public void RejectFriendRequest(string sessionToken, string requesterFriendCode)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(requesterFriendCode);

        throw new NotSupportedException(CallNotSupported);
    }

    public void CancelFriendRequest(string sessionToken, string recipientFriendCode)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(recipientFriendCode);

        throw new NotSupportedException(CallNotSupported);
    }

    public void RemoveFriend(string sessionToken, string friendCode)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(friendCode);

        throw new NotSupportedException(CallNotSupported);
    }
}
