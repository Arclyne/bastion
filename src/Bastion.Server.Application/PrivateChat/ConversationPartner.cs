using Bastion.Server.Application.Friends;

namespace Bastion.Server.Application.PrivateChat;

public sealed record ConversationPartner(PlayerIdentity Player, bool IsFriend);
