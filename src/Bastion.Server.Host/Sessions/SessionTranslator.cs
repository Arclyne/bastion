using System;
using Bastion.Contracts.Sessions;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Host.Common;
using CoreWCF;

namespace Bastion.Server.Host.Sessions;

public static class SessionTranslator
{
    public static string RequireSessionToken(string? sessionToken)
    {
        if (sessionToken is null)
        {
            var missingSessionToken = new InvalidSessionException();
            throw ToFault(missingSessionToken);
        }

        return sessionToken;
    }

    public static FaultException<InvalidSessionFault> ToFault(InvalidSessionException rejection)
    {
        ArgumentNullException.ThrowIfNull(rejection);

        var fault = new InvalidSessionFault();
        return FaultExceptionFactory.Create(fault, rejection);
    }
}
