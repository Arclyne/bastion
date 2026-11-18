using System;
using System.Collections.Generic;
using System.ServiceModel;
using Bastion.Client.Networking.Channels;
using Bastion.Contracts.Friends;

namespace Bastion.Client.Networking.Friends;

public sealed class FriendClient : IFriendClient
{
    private readonly IServiceChannel<IFriendshipService> _channel;

    public FriendClient(IServiceChannel<IFriendshipService> channel)
    {
        ArgumentNullException.ThrowIfNull(channel);

        _channel = channel;
    }

    public FriendListing ListFriends(string sessionToken)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        Func<IFriendshipService, FriendList> listFriends = service => service.ListFriends(sessionToken);
        FriendList list = Call(listFriends);
        return FriendContractTranslator.ToFriendListing(list);
    }

    public IReadOnlyList<FoundPlayer> SearchPlayers(string sessionToken, string text)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(text);

        Func<IFriendshipService, PlayerSearchEntry[]> searchPlayers = service => service.SearchPlayers(
            sessionToken,
            text);
        PlayerSearchEntry[] entries = Call(searchPlayers);
        return FriendContractTranslator.ToFoundPlayers(entries);
    }

    public FriendRequestOutcome SendFriendRequest(string sessionToken, string friendCode)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(friendCode);

        Func<IFriendshipService, FriendRequestResult> sendFriendRequest = service => service.SendFriendRequest(
            sessionToken,
            friendCode);
        FriendRequestResult result = Call(sendFriendRequest);
        return FriendContractTranslator.ToOutcome(result);
    }

    public FriendRequestListing ListFriendRequests(string sessionToken)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        Func<IFriendshipService, FriendRequestList> listFriendRequests = service => service.ListFriendRequests(
            sessionToken);
        FriendRequestList list = Call(listFriendRequests);
        return FriendContractTranslator.ToFriendRequestListing(list);
    }

    public void AcceptFriendRequest(string sessionToken, string requesterFriendCode)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(requesterFriendCode);

        Action<IFriendshipService> acceptFriendRequest = service => service.AcceptFriendRequest(
            sessionToken,
            requesterFriendCode);
        Call(acceptFriendRequest);
    }

    public void RejectFriendRequest(string sessionToken, string requesterFriendCode)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(requesterFriendCode);

        Action<IFriendshipService> rejectFriendRequest = service => service.RejectFriendRequest(
            sessionToken,
            requesterFriendCode);
        Call(rejectFriendRequest);
    }

    public void CancelFriendRequest(string sessionToken, string recipientFriendCode)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(recipientFriendCode);

        Action<IFriendshipService> cancelFriendRequest = service => service.CancelFriendRequest(
            sessionToken,
            recipientFriendCode);
        Call(cancelFriendRequest);
    }

    public void RemoveFriend(string sessionToken, string friendCode)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(friendCode);

        Action<IFriendshipService> removeFriend = service => service.RemoveFriend(sessionToken, friendCode);
        Call(removeFriend);
    }

    private TResult Call<TResult>(Func<IFriendshipService, TResult> operation)
    {
        try
        {
            return _channel.Call(operation);
        }
        catch (CommunicationException ex)
        {
            throw ToClientException(ex);
        }
        catch (TimeoutException ex)
        {
            throw ToClientException(ex);
        }
    }

    private static Exception ToClientException(Exception cause)
    {
        if (cause is FaultException<FriendshipRejectedFault> rejected)
        {
            FriendshipRejection rejection = FriendContractTranslator.ToRejection(rejected.Detail.Reason);
            return new FriendshipRejectedException(rejection, cause);
        }

        return ServiceFailureTranslator.ToClientException(cause);
    }

    private void Call(Action<IFriendshipService> operation)
    {
        Func<IFriendshipService, bool> completeOperation = service => CompleteOperation(operation, service);
        Call(completeOperation);
    }

    private static bool CompleteOperation(Action<IFriendshipService> operation, IFriendshipService service)
    {
        operation(service);
        return true;
    }
}
