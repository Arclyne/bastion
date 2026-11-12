using System;
using System.Collections.Generic;
using Bastion.Client.Networking.Chat;

namespace Bastion.Client.Networking.PrivateChat;

public sealed record ConversationTranscript
{
    public ConversationTranscript(ConversationContact contact, IReadOnlyList<PrivateMessage> messages, ChatMute mute)
    {
        ArgumentNullException.ThrowIfNull(contact);
        ArgumentNullException.ThrowIfNull(messages);
        ArgumentNullException.ThrowIfNull(mute);

        Contact = contact;
        Messages = messages;
        Mute = mute;
    }

    public ConversationContact Contact { get; }

    public IReadOnlyList<PrivateMessage> Messages { get; }

    public ChatMute Mute { get; }
}
