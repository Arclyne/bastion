using System;
using System.ServiceModel;
using Bastion.Client.Networking.Sessions;
using Bastion.Contracts.Sessions;

namespace Bastion.Client.Networking.Channels;

public static class ServiceFailureTranslator
{
    public static Exception ToClientException(Exception cause)
    {
        ArgumentNullException.ThrowIfNull(cause);

        switch (cause)
        {
            case EndpointNotFoundException:
                return new ServerUnreachableException(cause);
            case FaultException<InvalidSessionFault>:
                return new InvalidSessionException(cause);
            case FaultException:
                return new ServerErrorException(cause);
            case CommunicationException:
            case TimeoutException:
                return new ConnectionLostException(cause);
            default:
                throw new ArgumentException($"Unsupported exception type. Type={cause.GetType().Name}", nameof(cause));
        }
    }
}
