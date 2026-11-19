using System;

namespace Bastion.Client.Networking.Matches;

public sealed class DrawOfferRejectedException : Exception
{
    public DrawOfferRejectedException(DrawOfferRefusal refusal, int movesLeft, Exception innerException)
        : base($"The draw offer was turned down. Refusal={refusal}; MovesLeft={movesLeft}", innerException)
    {
        ArgumentNullException.ThrowIfNull(innerException);

        Refusal = refusal;
        MovesLeft = movesLeft;
    }

    public DrawOfferRefusal Refusal { get; }

    public int MovesLeft { get; }
}
