using System;
using System.Runtime.Serialization;

namespace Bastion.Contracts.Chat;

[DataContract]
public sealed class LobbyChatMessage
{
    [DataMember]
    public long MessageId { get; set; }

    [DataMember]
    public string? SenderNickname { get; set; }

    [DataMember]
    public string? Text { get; set; }

    [DataMember]
    public DateTime SentAt { get; set; }

    [DataMember]
    public bool IsOwn { get; set; }
}
