namespace Bastion.Client.Networking.PrivateChat;

public sealed record ConversationPreview(ConversationContact Contact, PrivateMessage LastMessage, int UnreadCount);
