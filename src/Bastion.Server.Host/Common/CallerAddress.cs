using CoreWCF;
using CoreWCF.Channels;

namespace Bastion.Server.Host.Common;

public static class CallerAddress
{
    private const string UnknownAddress = "0.0.0.0";

    public static string GetCurrent()
    {
        MessageProperties? incomingProperties = OperationContext.Current?.IncomingMessageProperties;
        if (incomingProperties is null)
        {
            return UnknownAddress;
        }

        return ReadRemoteAddress(incomingProperties);
    }

    private static string ReadRemoteAddress(MessageProperties incomingProperties)
    {
        bool hasRemoteEndpoint = incomingProperties.TryGetValue(RemoteEndpointMessageProperty.Name, out object? property);
        if (!hasRemoteEndpoint)
        {
            return UnknownAddress;
        }

        var remoteEndpoint = (RemoteEndpointMessageProperty)property;
        return remoteEndpoint.Address;
    }
}
