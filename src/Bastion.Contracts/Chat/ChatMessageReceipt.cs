using System;
using System.Runtime.Serialization;

namespace Bastion.Contracts.Chat;

[DataContract]
public sealed class ChatMessageReceipt
{
    [DataMember]
    public long MessageId { get; set; }

    [DataMember]
    public DateTime SentAt { get; set; }
}
