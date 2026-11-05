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
