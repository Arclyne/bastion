using System.Runtime.Serialization;

namespace Bastion.Contracts.Notifications;

[DataContract]
public sealed class MatchFoundParticipant
{
    [DataMember]
    public string? Nickname { get; set; }

    [DataMember]
    public short? EloRating { get; set; }

    [DataMember]
    public byte TurnOrder { get; set; }
}
