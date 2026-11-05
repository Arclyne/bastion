namespace Bastion.Server.Application.Chat;

public sealed record ChatHistoryEntry(PostedChatLine Line, bool IsOwn);
