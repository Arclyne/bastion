using Bastion.Domain.Chat;
using Bastion.Server.Application.Friends;

namespace Bastion.Server.Application.PrivateChat;

public sealed record PrivateMessageDelivery(PlayerIdentity Author, Message Message);
