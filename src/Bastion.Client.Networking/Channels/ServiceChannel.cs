using System;
using System.ServiceModel;

namespace Bastion.Client.Networking.Channels;

public sealed class ServiceChannel<TContract> : IDisposable
    where TContract : class
{
    private readonly ChannelFactory<TContract> _channelFactory;

    public ServiceChannel(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        _channelFactory = new ChannelFactory<TContract>(ChannelSettings.CreateBinding(), ChannelSettings.CreateAddress(path));
        ChannelSettings.ApplyCertificateValidation(_channelFactory);
    }

    public void Call(Action<TContract> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        Call(channel => InvokeAndReturnTrue(operation, channel));
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

    private static bool InvokeAndReturnTrue(Action<TContract> operation, TContract channel)
    {
        operation(channel);
        return true;
    }
}
