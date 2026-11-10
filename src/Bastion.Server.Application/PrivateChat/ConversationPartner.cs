using Bastion.Server.Application.Accounts;

namespace Bastion.Server.Application.PrivateChat;

public sealed record ConversationPartner(PlayerIdentity Player, bool IsFriend);
