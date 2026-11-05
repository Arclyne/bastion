using System.Runtime.Serialization;

namespace Bastion.Contracts.Notifications;

[DataContract]
public sealed class SearchEndedNotification : Notification
{
    [DataMember]
    public SearchEndedReason Reason { get; set; }
}
