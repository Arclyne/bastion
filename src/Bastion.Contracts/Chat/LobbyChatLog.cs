using System;
using System.Runtime.Serialization;

namespace Bastion.Contracts.Chat;

[DataContract]
public sealed class LobbyChatLog
{
    [DataMember]
    public LobbyChatMessage[]? Messages { get; set; }

    [DataMember]
    public int ConnectedPlayers { get; set; }

    [DataMember]
    public bool IsMuted { get; set; }

    [DataMember]
    public DateTime? MutedUntil { get; set; }
}
