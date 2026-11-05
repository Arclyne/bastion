using System.Collections.Generic;

namespace Bastion.Server.Application.Chat;

public sealed record LobbyChatHistory(IReadOnlyList<ChatHistoryEntry> Entries, int ConnectedPlayers, ChatMute Mute);
