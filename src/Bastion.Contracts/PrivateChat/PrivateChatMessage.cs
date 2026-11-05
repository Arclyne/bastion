using System;
using System.Runtime.Serialization;

namespace Bastion.Contracts.PrivateChat;

[DataContract]
public sealed class PrivateChatMessage
{
    [DataMember]
    public long MessageId { get; set; }

    [DataMember]
    public string? Text { get; set; }

    [DataMember]
    public DateTime SentAt { get; set; }

    [DataMember]
    public bool IsOwn { get; set; }
}
