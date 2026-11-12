using System.Runtime.Serialization;

namespace Bastion.Contracts.Matches;

[DataContract]
public sealed class NotYourTurnFault
{
    [DataMember]
    public byte CurrentTurnOrder { get; set; }

    [DataMember]
    public short NextMoveNumber { get; set; }
}
