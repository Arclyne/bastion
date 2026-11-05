using Bastion.Client.Networking.Friends;

namespace Bastion.Client.Networking.PrivateChat;

public sealed record ConversationContact(PlayerTag Partner, bool IsFriend);
