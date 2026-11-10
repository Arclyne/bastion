using System;
using System.Collections.Generic;
using Bastion.Server.Application.Chat;

namespace Bastion.Server.Application.PrivateChat;

public sealed record PrivateConversation
{
    public PrivateConversation(
        ConversationPartner partner,
        IReadOnlyList<PrivateMessageEntry> messages,
        ChatMute mute)
    {
        ArgumentNullException.ThrowIfNull(partner);
        ArgumentNullException.ThrowIfNull(messages);
        ArgumentNullException.ThrowIfNull(mute);

        Partner = partner;
        Messages = messages;
        Mute = mute;
    }

    public ConversationPartner Partner { get; }

    public IReadOnlyList<PrivateMessageEntry> Messages { get; }

    public ChatMute Mute { get; }
}
