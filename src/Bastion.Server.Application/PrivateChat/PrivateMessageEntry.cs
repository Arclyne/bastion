using Bastion.Domain.Chat;

namespace Bastion.Server.Application.PrivateChat;

public sealed record PrivateMessageEntry(Message Message, bool IsOwn);
