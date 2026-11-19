namespace Bastion.Domain.Matches;

public sealed record DrawOfferRejection(DrawOfferRejectionReason Reason, int MovesLeft)
{
    public static DrawOfferRejection None { get; } = new DrawOfferRejection(DrawOfferRejectionReason.None, 0);
}
