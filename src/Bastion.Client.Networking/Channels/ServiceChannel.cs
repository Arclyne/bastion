using System;
using System.ServiceModel;

namespace Bastion.Client.Networking.Channels;

public sealed class ServiceChannel<TContract> : IDisposable
    where TContract : class
{
    private readonly ChannelFactory<TContract> _factory;

    public ServiceChannel(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        _factory = new ChannelFactory<TContract>(ChannelSettings.CreateBinding(), ChannelSettings.CreateAddress(path));
        ChannelSettings.ApplyCertificateValidation(_factory);
    }

    public void Call(Action<TContract> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        Call(channel => Invoke(operation, channel));
    }

    public TResult Call<TResult>(Func<TContract, TResult> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        TContract channel = _factory.CreateChannel();
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
        ChannelCloser.CloseOrAbort(_factory);
    }

    private static bool Invoke(Action<TContract> operation, TContract channel)
    {
        operation(channel);
        return true;
    }
}
