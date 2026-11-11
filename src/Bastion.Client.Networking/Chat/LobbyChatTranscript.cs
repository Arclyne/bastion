using System.Collections.Generic;

namespace Bastion.Client.Networking.Chat;

public sealed record LobbyChatTranscript(IReadOnlyList<ChatMessage> Messages, int ConnectedPlayerCount, ChatMute Mute);
