using System.Runtime.Serialization;
using Bastion.Contracts.Notifications;

namespace Bastion.Contracts.Matches;

[DataContract]
public sealed class MatchSnapshot
{
    [DataMember]
    public int MatchId { get; set; }

    [DataMember]
    public byte BoardSize { get; set; }

    [DataMember]
    public byte WallsPerPlayer { get; set; }

    [DataMember]
    public byte ClockMinutes { get; set; }

    [DataMember]
    public SnapshotPlayer[]? Players { get; set; }

    [DataMember]
    public SnapshotMove[]? Moves { get; set; }

    [DataMember]
    public byte CurrentTurnOrder { get; set; }

    [DataMember]
    public short NextMoveNumber { get; set; }

    [DataMember]
    public int TurnElapsedMilliseconds { get; set; }

    [DataMember]
    public byte PendingDrawOffererTurnOrder { get; set; }

    [DataMember]
    public int DrawMovesLeft { get; set; }

    [DataMember]
    public MatchEndedNotification? Ending { get; set; }

    [DataMember]
    public ParticipationEndedNotification? OwnDeparture { get; set; }
}
