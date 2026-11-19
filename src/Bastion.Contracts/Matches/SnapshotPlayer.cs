using System.Runtime.Serialization;

namespace Bastion.Contracts.Matches;

[DataContract]
public sealed class SnapshotPlayer
{
    [DataMember]
    public byte TurnOrder { get; set; }

    [DataMember]
    public string? Nickname { get; set; }

    [DataMember]
    public short? EloRating { get; set; }

    [DataMember]
    public string? Square { get; set; }

    [DataMember]
    public byte RemainingWalls { get; set; }

    [DataMember]
    public int? RemainingClockMilliseconds { get; set; }

    [DataMember]
    public bool HasEnded { get; set; }

    [DataMember]
    public bool IsConnected { get; set; }

    [DataMember]
    public int DisconnectedMilliseconds { get; set; }
}
