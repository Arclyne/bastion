using System.Runtime.Serialization;

namespace Bastion.Contracts.Notifications;

[DataContract]
public sealed class PawnMovedNotification : Notification
{
    [DataMember]
    public int MatchId { get; set; }

    [DataMember]
    public string? MoverNickname { get; set; }

    [DataMember]
    public string? FromSquare { get; set; }

    [DataMember]
    public string? ToSquare { get; set; }

    [DataMember]
    public byte NextTurnOrder { get; set; }

    [DataMember]
    public short NextMoveNumber { get; set; }

    [DataMember]
    public int? RemainingClockMilliseconds { get; set; }

    [DataMember]
    public bool IsMatchOver { get; set; }
}
