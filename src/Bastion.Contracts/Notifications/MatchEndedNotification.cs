using System.Runtime.Serialization;
using Bastion.Contracts.Matches;

namespace Bastion.Contracts.Notifications;

[DataContract]
public sealed class MatchEndedNotification : Notification
{
    [DataMember]
    public int MatchId { get; set; }

    [DataMember]
    public MatchEndReason EndReason { get; set; }

    [DataMember]
    public byte DecidingTurnOrder { get; set; }

    [DataMember]
    public bool IsRanked { get; set; }

    [DataMember]
    public int MoveCount { get; set; }

    [DataMember]
    public int PlayedSeconds { get; set; }

    [DataMember]
    public MatchStanding[]? Standings { get; set; }
}
