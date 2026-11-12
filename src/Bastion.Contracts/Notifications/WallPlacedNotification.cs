using System.Runtime.Serialization;
using Bastion.Contracts.Matches;

namespace Bastion.Contracts.Notifications;

[DataContract]
public sealed class WallPlacedNotification : Notification
{
    [DataMember]
    public int MatchId { get; set; }

    [DataMember]
    public string? PlacerNickname { get; set; }

    [DataMember]
    public string? Groove { get; set; }

    [DataMember]
    public WallOrientation Orientation { get; set; }

    [DataMember]
    public byte RemainingWalls { get; set; }

    [DataMember]
    public byte NextTurnOrder { get; set; }

    [DataMember]
    public short NextMoveNumber { get; set; }

    [DataMember]
    public int? RemainingClockMilliseconds { get; set; }
}
