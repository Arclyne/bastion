using System;
using System.Collections.Generic;
using Bastion.Server.Application.Chat;

namespace Bastion.Server.Application.PrivateChat;

public sealed record PrivateConversation
{
    public PrivateConversation(
        ConversationPartner partner,
        IReadOnlyList<PrivateMessageEntry> messages,
        ChatMuteStatus muteStatus)
    {
        ArgumentNullException.ThrowIfNull(partner);
        ArgumentNullException.ThrowIfNull(messages);
        ArgumentNullException.ThrowIfNull(muteStatus);

        Partner = partner;
        Messages = messages;
        MuteStatus = muteStatus;
    }

    public ConversationPartner Partner { get; }

    public IReadOnlyList<PrivateMessageEntry> Messages { get; }

    public ChatMuteStatus MuteStatus { get; }
}
