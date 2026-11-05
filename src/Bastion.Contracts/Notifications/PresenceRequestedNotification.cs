using System;
using System.Runtime.Serialization;

namespace Bastion.Contracts.Notifications;

[DataContract]
public sealed class PresenceRequestedNotification : Notification
{
    [DataMember]
    public DateTime Deadline { get; set; }
}
