using System;
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

    public void Dispose()
    {
        _channel.Dispose();
    }

    private TResult Call<TResult>(Func<IFriendshipService, TResult> operation)
    {
        try
        {
            return _channel.Call(operation);
        }
        catch (CommunicationException ex)
        {
            throw ServiceFailureTranslator.ToClientException(ex);
        }
        catch (TimeoutException ex)
        {
            throw ServiceFailureTranslator.ToClientException(ex);
        }
    }
}
