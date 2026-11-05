using System;
using System.Runtime.Serialization;

namespace Bastion.Contracts.Chat;

[DataContract]
public sealed class ChatMessageRejectedFault
{
    [DataMember]
    public ChatRejectionReason Reason { get; set; }

    [DataMember]
    public DateTime? MutedUntil { get; set; }
}
