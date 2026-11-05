using System.Runtime.Serialization;

namespace Bastion.Contracts.Notifications;

[DataContract]
public enum SearchEndedReason
{
    [EnumMember]
    Cancelled,

    [EnumMember]
    PresenceExpired,

    [EnumMember]
    MatchCreationFailed,
}
