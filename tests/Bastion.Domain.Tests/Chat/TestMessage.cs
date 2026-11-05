using System;
using Bastion.Domain.Chat;
using Xunit;

namespace Bastion.Domain.Tests.Chat;

public class TestMessage
{
    private const int AuthorId = 3;
    private const int RecipientId = 4;
    private const int MatchId = 16;
    private const string MatchChatText = "good luck";

    private static readonly DateTime _sentAt = new DateTime(2026, 11, 5, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Constructor_PrivateMessage_UsesFriendsChannel()
    {
        Message message = CreateMessage();

        Assert.Equal(MessageChannel.Friends, message.Channel);
    }

    [Fact]
    public void Constructor_SameAuthorAndRecipient_Throws()
    {
        var participants = new MessageParticipants(AuthorId, AuthorId);

        Assert.Throws<ArgumentException>(() => new Message(participants, "hola", _sentAt));
    }

    [Fact]
    public void GetPartnerId_Author_ReturnsRecipient()
    {
        Message message = CreateMessage();

        int partnerId = message.GetPartnerId(AuthorId);

        Assert.Equal(RecipientId, partnerId);
    }

    [Fact]
    public void GetPartnerId_Recipient_ReturnsAuthor()
    {
        Message message = CreateMessage();

        int partnerId = message.GetPartnerId(RecipientId);

        Assert.Equal(AuthorId, partnerId);
    }

    [Fact]
    public void MarkRead_AlreadyRead_KeepsFirstMoment()
    {
        Message message = CreateMessage();
        message.MarkRead(_sentAt.AddMinutes(1));

        message.MarkRead(_sentAt.AddMinutes(5));

        Assert.Equal(_sentAt.AddMinutes(1), message.ReadAt);
    }

    private static Message CreateMessage()
    {
        return new Message(new MessageParticipants(AuthorId, RecipientId), "hola", _sentAt);
    }

    [Fact]
    public void CreateForMatch_MatchSubmission_UsesMatchChannel()
    {
        var submission = new MatchChatSubmission(MatchId, MatchChatText);

        var message = Message.CreateForMatch(submission, AuthorId, _sentAt);

        Assert.Equal(MessageChannel.Match, message.Channel);
    }

    [Fact]
    public void CreateForMatch_MatchSubmission_KeepsMatchId()
    {
        var submission = new MatchChatSubmission(MatchId, MatchChatText);

        var message = Message.CreateForMatch(submission, AuthorId, _sentAt);

        Assert.Equal(MatchId, message.MatchId);
    }
}
