using System;
using CoreWCF;
using Bastion.Contracts.Sessions;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Host.Sessions;

public static class SessionTranslator
{
    public static FaultException<InvalidSessionFault> ToFault(InvalidSessionException rejection)
    {
        ArgumentNullException.ThrowIfNull(rejection);

        return new FaultException<InvalidSessionFault>(new InvalidSessionFault(), new FaultReason(rejection.Message));
    }
}
