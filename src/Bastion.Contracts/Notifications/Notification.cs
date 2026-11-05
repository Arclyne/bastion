using System;
using System.Runtime.Serialization;

namespace Bastion.Contracts.Notifications;

[DataContract]
[KnownType(typeof(ChatMessageNotification))]
[KnownType(typeof(MatchFoundNotification))]
[KnownType(typeof(PresenceRequestedNotification))]
[KnownType(typeof(SearchEndedNotification))]
[KnownType(typeof(FriendPresenceNotification))]
public abstract class Notification
{
    [DataMember]
    public DateTime OccurredAt { get; set; }
}
