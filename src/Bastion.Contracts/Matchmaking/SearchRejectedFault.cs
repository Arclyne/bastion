using System.Runtime.Serialization;

namespace Bastion.Contracts.Matchmaking;

[DataContract]
public sealed class SearchRejectedFault
{
    [DataMember]
    public SearchRejectionReason Reason { get; set; }
}
