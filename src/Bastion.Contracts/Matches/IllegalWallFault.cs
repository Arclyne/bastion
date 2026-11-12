using System.Runtime.Serialization;

namespace Bastion.Contracts.Matches;

[DataContract]
public sealed class IllegalWallFault
{
    [DataMember]
    public WallRejectionReason Reason { get; set; }

    [DataMember]
    public byte? BlockedTurnOrder { get; set; }
}
