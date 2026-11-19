using System;
using System.Runtime.Serialization;

namespace Bastion.Contracts.Notifications;

[DataContract]
[KnownType(typeof(ChatMessageNotification))]
[KnownType(typeof(MatchFoundNotification))]
[KnownType(typeof(MatchChatMessageNotification))]
[KnownType(typeof(MatchEndedNotification))]
[KnownType(typeof(ParticipationEndedNotification))]
[KnownType(typeof(PawnMovedNotification))]
[KnownType(typeof(WallPlacedNotification))]
[KnownType(typeof(PresenceRequestedNotification))]
[KnownType(typeof(SearchEndedNotification))]
[KnownType(typeof(FriendPresenceNotification))]
[KnownType(typeof(FriendRequestNotification))]
[KnownType(typeof(FriendshipStartedNotification))]
[KnownType(typeof(PrivateMessageNotification))]
[KnownType(typeof(RoomUpdatedNotification))]
[KnownType(typeof(RoomClosedNotification))]
[KnownType(typeof(OpponentDisconnectedNotification))]
[KnownType(typeof(OpponentReconnectedNotification))]
[KnownType(typeof(MatchTakenOverNotification))]
[KnownType(typeof(DrawOfferedNotification))]
[KnownType(typeof(DrawRejectedNotification))]
[KnownType(typeof(DrawOfferExpiredNotification))]
public abstract class Notification
{
    [DataMember]
    public DateTime OccurredAt { get; set; }
}
