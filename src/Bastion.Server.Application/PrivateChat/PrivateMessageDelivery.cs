using Bastion.Domain.Chat;
using Bastion.Server.Application.Accounts;

namespace Bastion.Server.Application.PrivateChat;

public sealed record PrivateMessageDelivery(PlayerIdentity Author, Message Message);
