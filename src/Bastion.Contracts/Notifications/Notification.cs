using System;
using System.Runtime.Serialization;

namespace Bastion.Contracts.Notifications;

[DataContract]
[KnownType(typeof(ChatMessageNotification))]
public abstract class Notification
{
    [DataMember]
    public DateTime OccurredAt { get; set; }
}
