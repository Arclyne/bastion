using System;
using System.ServiceModel;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Sessions;
using Bastion.Contracts.Sessions;

namespace Bastion.Client.Networking.Channels;

public static class ServiceFailureTranslator
{
    public static Exception ToClientException(Exception ex)
    {
        ArgumentNullException.ThrowIfNull(ex);

        switch (ex)
        {
            case EndpointNotFoundException:
                return new ServerUnreachableException(ex);
            case FaultException<InvalidSessionFault>:
                return new InvalidSessionException(ex);
            case FaultException:
                return new ServerErrorException(ex);
            case CommunicationException:
            case TimeoutException:
                return new ConnectionLostException(ex);
            default:
                throw new ArgumentOutOfRangeException(nameof(ex), ex.GetType(), "Unsupported exception type.");
        }
    }
}
