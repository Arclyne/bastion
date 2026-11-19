using System;
using Bastion.Domain.Matches;

namespace Bastion.Server.Application.Matches;

public sealed class DrawOfferRejectedException : Exception
{
    public DrawOfferRejectedException(DrawOfferRejection rejection)
        : base($"The draw cannot be offered. Reason={rejection?.Reason}; MovesLeft={rejection?.MovesLeft}")
    {
        ArgumentNullException.ThrowIfNull(rejection);

        Reason = rejection.Reason;
        MovesLeft = rejection.MovesLeft;
    }

    public DrawOfferRejectionReason Reason { get; }

    public int MovesLeft { get; }
}
