using System;
using Bastion.Client.Networking.Chat;
using Bastion.Client.Networking.Notifications;

namespace Bastion.Client.Tests.Fakes;

public sealed class FakeChatClient : IChatClient
{
    private const string LobbyChatNotSupported = "The tests that use this fake never load the lobby chat.";

    public static DateTime ConfirmedSentAt { get; } = new DateTime(2026, 11, 5, 18, 2, 0, DateTimeKind.Utc);

    public Exception? LobbyChatSendFailure { get; set; }

    public Exception? MatchChatSendFailure { get; set; }

    public int MatchChatSendCount { get; private set; }

    public int? LastSentMatchId { get; private set; }

    public string? LastSentText { get; private set; }

    public event EventHandler<MatchChatMessageReceivedEventArgs>? MatchChatMessageReceived;

    public DateTime SendLobbyChatMessage(string sessionToken, string text)
    {
        if (LobbyChatSendFailure is not null)
        {
            throw LobbyChatSendFailure;
        }

        LastSentText = text;
        return ConfirmedSentAt;
    }

    public DateTime SendMatchChatMessage(string sessionToken, int matchId, string text)
    {
        if (MatchChatSendFailure is not null)
        {
            throw MatchChatSendFailure;
        }

        MatchChatSendCount++;
        LastSentMatchId = matchId;
        LastSentText = text;
        return ConfirmedSentAt;
    }

    public LobbyChatTranscript OpenLobbyChat(string sessionToken)
    {
        throw new NotSupportedException(LobbyChatNotSupported);
    }

    public bool HasMatchChatSubscribers()
    {
        return MatchChatMessageReceived is not null;
    }

    public void RaiseMatchChatMessageReceived(int matchId, ChatMessage message)
    {
        var receivedMessage = new MatchChatMessageReceivedEventArgs(matchId, message);
        MatchChatMessageReceived?.Invoke(this, receivedMessage);
    }
}
