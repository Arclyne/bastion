namespace Bastion.Server.Application.PrivateChat;

public sealed record ConversationSummary(ConversationPartner Partner, PrivateMessageEntry LastMessage, int UnreadCount);
