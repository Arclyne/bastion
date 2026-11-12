using System;
using Bastion.Client.Networking.Chat;
using Bastion.Client.Networking.Notifications;

namespace Bastion.Client.Tests.Fakes;

public sealed class FakeChatClient : IChatClient
{
    private const string LobbyChatNotSupported = "The tests that use this fake never use the lobby chat.";

    private static readonly DateTime _confirmedSentAt = new DateTime(2026, 11, 5, 18, 2, 0, DateTimeKind.Utc);

    public Exception? MatchChatSendFailure { get; set; }

    public int MatchChatSendCount { get; private set; }

    public int? LastSentMatchId { get; private set; }

    public event EventHandler<MatchChatMessageReceivedEventArgs>? MatchChatMessageReceived;

    public DateTime SendMessage(string sessionToken, string text)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(text);

        throw new NotSupportedException(LobbyChatNotSupported);
    }

    public DateTime SendMatchChatMessage(string sessionToken, int matchId, string text)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(text);

        if (MatchChatSendFailure is not null)
        {
            throw MatchChatSendFailure;
        }

        MatchChatSendCount++;
        LastSentMatchId = matchId;
        return _confirmedSentAt;
    }

    public LobbyChatTranscript OpenLobbyChat(string sessionToken)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        throw new NotSupportedException(LobbyChatNotSupported);
    }

    public bool HasMatchChatSubscribers()
    {
        return MatchChatMessageReceived is not null;
    }

    public void RaiseMatchChatMessageReceived(int matchId, ChatMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var receivedMessage = new MatchChatMessageReceivedEventArgs(matchId, message);
        MatchChatMessageReceived?.Invoke(this, receivedMessage);
    }
}
