using System.Runtime.Serialization;

namespace Bastion.Contracts.Matches;

[DataContract]
public sealed class DrawOfferRejectedFault
{
    [DataMember]
    public DrawOfferRejectionReason Reason { get; set; }

    [DataMember]
    public int MovesLeft { get; set; }
}
