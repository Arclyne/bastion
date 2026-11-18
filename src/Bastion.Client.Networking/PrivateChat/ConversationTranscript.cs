using System;
using System.Collections.Generic;
using Bastion.Client.Networking.Chat;

namespace Bastion.Client.Networking.PrivateChat;

public sealed record ConversationTranscript
{
    public ConversationTranscript(
        ConversationContact contact,
        IReadOnlyList<PrivateMessage> messages,
        ChatMuteStatus muteStatus)
    {
        ArgumentNullException.ThrowIfNull(contact);
        ArgumentNullException.ThrowIfNull(messages);
        ArgumentNullException.ThrowIfNull(muteStatus);

        Contact = contact;
        Messages = messages;
        MuteStatus = muteStatus;
    }

    public ConversationContact Contact { get; }

    public IReadOnlyList<PrivateMessage> Messages { get; }

    public ChatMuteStatus MuteStatus { get; }
}
