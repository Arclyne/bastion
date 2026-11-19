using System.Runtime.Serialization;
using Bastion.Contracts.Matches;

namespace Bastion.Contracts.Notifications;

[DataContract]
public sealed class ParticipationEndedNotification : Notification
{
    [DataMember]
    public int MatchId { get; set; }

    [DataMember]
    public MatchEndReason EndReason { get; set; }

    [DataMember]
    public byte DepartingTurnOrder { get; set; }

    [DataMember]
    public int ActivePlayerCount { get; set; }

    [DataMember]
    public byte NextTurnOrder { get; set; }

    [DataMember]
    public short NextMoveNumber { get; set; }

    [DataMember]
    public int MoveCount { get; set; }

    [DataMember]
    public int PlayedSeconds { get; set; }

    [DataMember]
    public MatchStanding[]? Standings { get; set; }
}
