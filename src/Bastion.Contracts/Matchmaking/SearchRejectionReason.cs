using System.Runtime.Serialization;

namespace Bastion.Contracts.Matchmaking;

[DataContract]
public enum SearchRejectionReason
{
    [EnumMember]
    NotificationsUnavailable,
}
