using System.Collections.Generic;
using Bastion.Server.Application.Chat;

namespace Bastion.Server.Application.PrivateChat;

public sealed record PrivateConversation(
    ConversationPartner Partner,
    IReadOnlyList<PrivateMessageEntry> Messages,
    ChatMute Mute);
