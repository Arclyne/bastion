using System;

namespace Bastion.Domain.Matches;

public class DrawOffer
{
    internal DrawOffer(int offererId, short moveNumber, DateTime offeredAt)
    {
        OffererId = offererId;
        MoveNumber = moveNumber;
        OfferedAt = offeredAt;
        Status = DrawOfferStatus.Pending;
    }

    private DrawOffer()
    {
    }

    public int DrawOfferId { get; private set; }

    public int MatchId { get; private set; }

    public int OffererId { get; private set; }

    public short MoveNumber { get; private set; }

    public DateTime OfferedAt { get; private set; }

    public DrawOfferStatus Status { get; private set; }

    public DateTime? RespondedAt { get; private set; }

    public bool IsPending()
    {
        return Status == DrawOfferStatus.Pending;
    }

    internal void Accept(DateTime acceptedAt)
    {
        Respond(DrawOfferStatus.Accepted, acceptedAt);
    }

    internal void Reject(DateTime rejectedAt)
    {
        Respond(DrawOfferStatus.Rejected, rejectedAt);
    }

    internal void Expire(DateTime expiredAt)
    {
        Respond(DrawOfferStatus.Expired, expiredAt);
    }

    private void Respond(DrawOfferStatus status, DateTime respondedAt)
    {
        if (!IsPending())
        {
            throw new InvalidOperationException("Only a pending draw offer can be answered.");
        }

        Status = status;
        RespondedAt = respondedAt;
    }
}
