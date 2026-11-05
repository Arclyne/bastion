using System;
using Bastion.Domain.Chat;
using Xunit;

namespace Bastion.Domain.Tests.Chat;

public class TestChatMessage
{
    private const int MatchId = 16;
    private const int AuthorAccountId = 12;
    private const string MessageText = "good luck";
    private const string PaddedMessageText = "  good luck  ";

    private static readonly DateTime _sentAt = new DateTime(2026, 11, 5, 20, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void WriteInMatch_MatchSubmission_UsesMatchChannel()
    {
        var submission = new MatchChatSubmission(MatchId, MessageText);

        var message = ChatMessage.WriteInMatch(submission, AuthorAccountId, _sentAt);

        Assert.Equal(ChatChannel.Match, message.Channel);
    }

    [Fact]
    public void WriteInMatch_MatchSubmission_KeepsTheMatch()
    {
        var submission = new MatchChatSubmission(MatchId, MessageText);

        var message = ChatMessage.WriteInMatch(submission, AuthorAccountId, _sentAt);

        Assert.Equal(MatchId, message.MatchId);
    }

    [Fact]
    public void WriteInMatch_PaddedText_TrimsText()
    {
        var submission = new MatchChatSubmission(MatchId, PaddedMessageText);

        var message = ChatMessage.WriteInMatch(submission, AuthorAccountId, _sentAt);

        Assert.Equal(MessageText, message.Text);
    }
}
