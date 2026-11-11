using System;
using CoreWCF;

namespace Bastion.Server.Host.Common;

public static class FaultExceptionFactory
{
    public static FaultException<TDetail> Create<TDetail>(TDetail detail, Exception rejection)
        where TDetail : class
    {
        ArgumentNullException.ThrowIfNull(detail);
        ArgumentNullException.ThrowIfNull(rejection);

        var reason = new FaultReason(rejection.Message);
        return new FaultException<TDetail>(detail, reason);
    }
}
