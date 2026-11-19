using System;
using System.Collections.Generic;
using Bastion.Client.Networking.Chat;
using Bastion.Client.Networking.Friends;
using Bastion.Client.Networking.PrivateChat;

namespace Bastion.Client.Tests.Fakes;

public sealed class FakeConversationClient : IConversationClient
{
    private const string CallNotSupported = "The tests that use this fake never list or mark private conversations.";

    private static readonly ChatMuteStatus _notMuted = new ChatMuteStatus(IsMuted: false, EndsAt: null);

    public static DateTime ConfirmedSentAt { get; } = new DateTime(2026, 11, 12, 10, 5, 0, DateTimeKind.Utc);

    public Exception? SendFailure { get; set; }

    public string? LastSentFriendCode { get; private set; }

    public string? LastSentText { get; private set; }

    public IReadOnlyList<ConversationPreview> ListConversations(string sessionToken)
    {
        throw new NotSupportedException(CallNotSupported);
    }

    public ConversationTranscript OpenConversation(string sessionToken, string friendCode)
    {
        var partner = new PlayerTag(string.Empty, friendCode);
        var contact = new ConversationContact(partner, isFriend: true);
        return new ConversationTranscript(contact, [], _notMuted);
    }

    public DateTime SendMessage(string sessionToken, string friendCode, string text)
    {
        if (SendFailure is not null)
        {
            throw SendFailure;
        }

        LastSentFriendCode = friendCode;
        LastSentText = text;
        return ConfirmedSentAt;
    }

    public void MarkConversationRead(string sessionToken, string friendCode)
    {
        throw new NotSupportedException(CallNotSupported);
    }
}
