using System;
using System.Collections.Generic;
using Bastion.Domain.Chat;

namespace Bastion.Server.Application.PrivateChat;

public interface IPrivateChatService
{
    IReadOnlyList<ConversationSummary> ListConversations(string sessionToken, DateTime now);

    PrivateConversation OpenConversation(string sessionToken, string friendCode, DateTime now);

    Message SendPrivateMessage(string sessionToken, PrivateMessageDraft draft, DateTime now);

    void MarkConversationRead(string sessionToken, string friendCode, DateTime now);
}
