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

        NetTcpBinding binding = ChannelSettings.CreateBinding();
        EndpointAddress address = ChannelSettings.CreateAddress(path);
        _channelFactory = new ChannelFactory<TContract>(binding, address);
        ChannelSettings.ApplyCertificateValidation(_channelFactory);
    }

    public void Call(Action<TContract> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        Func<TContract, bool> operationAsFunction = channel => CompleteOperation(operation, channel);
        Call(operationAsFunction);
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
