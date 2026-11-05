using System;
using System.Runtime.Serialization;

namespace Bastion.Contracts.Notifications;

[DataContract]
[KnownType(typeof(ChatMessageNotification))]
[KnownType(typeof(MatchFoundNotification))]
[KnownType(typeof(MatchChatMessageNotification))]
[KnownType(typeof(PresenceRequestedNotification))]
[KnownType(typeof(SearchEndedNotification))]
[KnownType(typeof(FriendPresenceNotification))]
[KnownType(typeof(FriendRequestNotification))]
[KnownType(typeof(FriendshipStartedNotification))]
[KnownType(typeof(PrivateMessageNotification))]
public abstract class Notification
{
    [DataMember]
    public DateTime OccurredAt { get; set; }
}
