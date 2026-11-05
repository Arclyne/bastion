using System;
using System.Collections.Generic;
using System.ServiceModel;
using Bastion.Client.Networking.Channels;
using Bastion.Contracts;
using Bastion.Contracts.Friends;

namespace Bastion.Client.Networking.Friends;

public sealed class FriendClient : IFriendClient, IDisposable
{
    private readonly ServiceChannel<IFriendshipService> _channel =
        new ServiceChannel<IFriendshipService>(ServiceEndpoints.FriendsPath);

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

    public void Dispose()
    {
        _channel.Dispose();
    }

    private static Exception ToClientException(Exception ex)
    {
        if (ex is FaultException<FriendshipRejectedFault> rejected)
        {
            return new FriendshipRejectedException(FriendContractTranslator.ToRejection(rejected.Detail.Reason), ex);
        }

        return ServiceFailureTranslator.ToClientException(ex);
    }

    private void Call(Action<IFriendshipService> operation)
    {
        Call(service =>
        {
            operation(service);
            return true;
        });
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
}
