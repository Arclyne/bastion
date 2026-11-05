using System.Runtime.Serialization;

namespace Bastion.Contracts.Chat;

[DataContract]
public enum ChatRejectionReason
{
    [EnumMember]
    InvalidMessage,

    [EnumMember]
    Muted,

    [EnumMember]
    BannedTerm,

    [EnumMember]
    TooFast,
}
