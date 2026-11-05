using System.Collections.Generic;
using Bastion.Client.Networking.Chat;

namespace Bastion.Client.Networking.PrivateChat;

public sealed record ConversationTranscript(
    ConversationContact Contact,
    IReadOnlyList<PrivateMessage> Messages,
    ChatMute Mute);
