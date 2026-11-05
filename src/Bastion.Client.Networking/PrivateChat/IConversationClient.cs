using System;
using System.Collections.Generic;

namespace Bastion.Client.Networking.PrivateChat;

public interface IConversationClient
{
    IReadOnlyList<ConversationPreview> ListConversations(string sessionToken);

    ConversationTranscript OpenConversation(string sessionToken, string friendCode);

    DateTime SendMessage(string sessionToken, string friendCode, string text);

    void MarkConversationRead(string sessionToken, string friendCode);
}
