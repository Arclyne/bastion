using System;
using System.ServiceModel;

namespace Bastion.Client.Networking.Channels;

public sealed class ServiceChannel<TContract> : IServiceChannel<TContract>, IDisposable
    where TContract : class
{
    private readonly ChannelFactory<TContract> _channelFactory;

    public ServiceChannel(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        NetTcpBinding binding = ChannelConfiguration.CreateBinding();
        EndpointAddress address = ChannelConfiguration.CreateAddress(path);
        _channelFactory = new ChannelFactory<TContract>(binding, address);
        ChannelConfiguration.ApplyCertificateValidation(_channelFactory);
    }

    public void Call(Action<TContract> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        Func<TContract, bool> completeOperation = channel => CompleteOperation(operation, channel);
        Call(completeOperation);
    }

    public TResult Call<TResult>(Func<TContract, TResult> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        TContract channel = _channelFactory.CreateChannel();
        var communicationObject = (ICommunicationObject)channel;
        try
        {
            TResult result = operation(channel);
            communicationObject.Close();
            return result;
        }
        catch (CommunicationException)
        {
            communicationObject.Abort();
            throw;
        }
        catch (TimeoutException)
        {
            communicationObject.Abort();
            throw;
        }
    }

    public void Dispose()
    {
        ChannelCloser.CloseOrAbort(_channelFactory);
    }

    private static bool CompleteOperation(Action<TContract> operation, TContract channel)
    {
        operation(channel);
        return true;
    }
}
