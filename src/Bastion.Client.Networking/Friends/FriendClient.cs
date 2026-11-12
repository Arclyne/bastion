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

        FriendList list = Call(service => service.ListFriends(sessionToken));
        return FriendContractTranslator.ToFriendListing(list);
    }

    public IReadOnlyList<FoundPlayer> SearchPlayers(string sessionToken, string text)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(text);

        PlayerSearchEntry[] entries = Call(service => service.SearchPlayers(sessionToken, text));
        return FriendContractTranslator.ToFoundPlayers(entries);
    }

    public FriendRequestOutcome SendFriendRequest(string sessionToken, string friendCode)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(friendCode);

        FriendRequestResult result = Call(service => service.SendFriendRequest(sessionToken, friendCode));
        return FriendContractTranslator.ToOutcome(result);
    }

    public FriendRequestListing ListFriendRequests(string sessionToken)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        FriendRequestList list = Call(service => service.ListFriendRequests(sessionToken));
        return FriendContractTranslator.ToFriendRequestListing(list);
    }

    public void AcceptFriendRequest(string sessionToken, string requesterFriendCode)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(requesterFriendCode);

        Call(service => service.AcceptFriendRequest(sessionToken, requesterFriendCode));
    }

    public void RejectFriendRequest(string sessionToken, string requesterFriendCode)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(requesterFriendCode);

        Call(service => service.RejectFriendRequest(sessionToken, requesterFriendCode));
    }

    public void CancelFriendRequest(string sessionToken, string recipientFriendCode)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(recipientFriendCode);

        Call(service => service.CancelFriendRequest(sessionToken, recipientFriendCode));
    }

    public void RemoveFriend(string sessionToken, string friendCode)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(friendCode);

        Call(service => service.RemoveFriend(sessionToken, friendCode));
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

    private static Exception ToClientException(Exception ex)
    {
        if (ex is FaultException<FriendshipRejectedFault> rejected)
        {
            FriendshipRejection rejection = FriendContractTranslator.ToRejection(rejected.Detail.Reason);
            return new FriendshipRejectedException(rejection, ex);
        }

        return ServiceFailureTranslator.ToClientException(ex);
    }

    private void Call(Action<IFriendshipService> operation)
    {
        Func<IFriendshipService, bool> operationAsFunction = service => CompleteOperation(operation, service);
        Call(operationAsFunction);
    }

    private static bool CompleteOperation(Action<IFriendshipService> operation, IFriendshipService service)
    {
        operation(service);
        return true;
    }
}
