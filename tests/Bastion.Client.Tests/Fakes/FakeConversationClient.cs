using System;
using System.Collections.Generic;
using Bastion.Client.Networking.PrivateChat;

namespace Bastion.Client.Tests.Fakes;

public sealed class FakeConversationClient : IConversationClient
{
    private const string CallNotSupported = "The tests that use this fake never call the private chat service.";

    public IReadOnlyList<ConversationPreview> ListConversations(string sessionToken)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        throw new NotSupportedException(CallNotSupported);
    }

    public ConversationTranscript OpenConversation(string sessionToken, string friendCode)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(friendCode);

        throw new NotSupportedException(CallNotSupported);
    }

    public DateTime SendMessage(string sessionToken, string friendCode, string text)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(friendCode);
        ArgumentNullException.ThrowIfNull(text);

        throw new NotSupportedException(CallNotSupported);
    }

    public void MarkConversationRead(string sessionToken, string friendCode)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(friendCode);

        throw new NotSupportedException(CallNotSupported);
    }
}
