using System;
using System.Runtime.Serialization;

namespace Bastion.Contracts.Notifications;

[DataContract]
[KnownType(typeof(ChatMessageNotification))]
[KnownType(typeof(MatchFoundNotification))]
[KnownType(typeof(MatchChatMessageNotification))]
[KnownType(typeof(MatchAbandonedNotification))]
[KnownType(typeof(PawnMovedNotification))]
[KnownType(typeof(PresenceRequestedNotification))]
[KnownType(typeof(SearchEndedNotification))]
[KnownType(typeof(FriendPresenceNotification))]
[KnownType(typeof(FriendRequestNotification))]
[KnownType(typeof(FriendshipStartedNotification))]
[KnownType(typeof(PrivateMessageNotification))]
[KnownType(typeof(RoomUpdatedNotification))]
[KnownType(typeof(RoomClosedNotification))]
public abstract class Notification
{
    [DataMember]
    public DateTime OccurredAt { get; set; }
}
