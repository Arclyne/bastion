using System.Runtime.Serialization;
using Bastion.Contracts.Matches;

namespace Bastion.Contracts.Notifications;

[DataContract]
public sealed class MatchStanding
{
    [DataMember]
    public byte TurnOrder { get; set; }

    [DataMember]
    public ParticipationResult? Result { get; set; }

    [DataMember]
    public byte? Placement { get; set; }

    [DataMember]
    public MatchEndReason? EndReason { get; set; }

    [DataMember]
    public short? InitialElo { get; set; }

    [DataMember]
    public short? FinalElo { get; set; }

    [DataMember]
    public byte RemainingWalls { get; set; }
}
