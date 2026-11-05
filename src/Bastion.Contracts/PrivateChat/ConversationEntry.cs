using System.Runtime.Serialization;

namespace Bastion.Contracts.PrivateChat;

[DataContract]
public sealed class ConversationEntry
{
    [DataMember]
    public string? PartnerNickname { get; set; }

    [DataMember]
    public string? PartnerFriendCode { get; set; }

    [DataMember]
    public bool IsFriend { get; set; }

    [DataMember]
    public PrivateChatMessage? LastMessage { get; set; }

    [DataMember]
    public int UnreadCount { get; set; }
}
