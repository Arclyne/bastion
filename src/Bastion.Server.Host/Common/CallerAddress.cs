using CoreWCF;
using CoreWCF.Channels;

namespace Bastion.Server.Host.Common;

public static class CallerAddress
{
    private const string UnknownAddress = "0.0.0.0";

    public static string GetCurrent()
    {
        MessageProperties? properties = OperationContext.Current?.IncomingMessageProperties;
        if (properties is null
            || !properties.TryGetValue(RemoteEndpointMessageProperty.Name, out object? value)
            || value is not RemoteEndpointMessageProperty endpoint)
        {
            return UnknownAddress;
        }

        return endpoint.Address;
    }
}
