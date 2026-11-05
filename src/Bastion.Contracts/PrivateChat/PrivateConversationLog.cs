using System;
using System.Runtime.Serialization;

namespace Bastion.Contracts.PrivateChat;

[DataContract]
public sealed class PrivateConversationLog
{
    [DataMember]
    public string? PartnerNickname { get; set; }

    [DataMember]
    public string? PartnerFriendCode { get; set; }

    [DataMember]
    public bool IsFriend { get; set; }

    [DataMember]
    public PrivateChatMessage[]? Messages { get; set; }

    [DataMember]
    public bool IsMuted { get; set; }

    [DataMember]
    public DateTime? MutedUntil { get; set; }
}
