using System.Runtime.Serialization;

namespace Bastion.Contracts.Matches;

[DataContract]
public enum DrawOfferRejectionReason
{
    [EnumMember]
    NotTwoPlayers,

    [EnumMember]
    AlreadyOffered,

    [EnumMember]
    TooSoon,
}
