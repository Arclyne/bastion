using System;
using Bastion.Domain.Chat;
using Xunit;

namespace Bastion.Domain.Tests.Chat;

public sealed class TestMessage
{
    private const int AuthorId = 3;
    private const int RecipientId = 4;
    private const int MatchId = 16;
    private const string PrivateText = "hello";
    private const string MatchChatText = "good luck";
    private const int FirstReadMinutes = 1;
    private const int SecondReadMinutes = 5;

    private static readonly DateTime _sentAt = new DateTime(2026, 11, 5, 12, 0, 0, DateTimeKind.Utc);
    private static readonly MatchChatSubmission _matchChatSubmission = new MatchChatSubmission(MatchId, MatchChatText);
    private static readonly MessageRoute _route = new MessageRoute(AuthorId, RecipientId);

    [Fact]
    public void CreatePrivate_DifferentPlayers_UsesFriendsChannel()
    {
        var message = Message.CreatePrivate(_route, PrivateText, _sentAt);

        Assert.Equal(MessageChannel.Friends, message.Channel);
    }

    [Fact]
    public void CreatePrivate_SameAuthorAndRecipient_ThrowsArgumentException()
    {
        var route = new MessageRoute(AuthorId, AuthorId);

        Action createSelfMessage = () => Message.CreatePrivate(route, PrivateText, _sentAt);

        Assert.Throws<ArgumentException>(createSelfMessage);
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
    public void GetPartnerId_MatchMessageAuthor_ThrowsInvalidOperationException()
    {
        var message = Message.CreateForMatch(_matchChatSubmission, AuthorId, _sentAt);

        Action getPartnerId = () => message.GetPartnerId(AuthorId);

        Assert.Throws<InvalidOperationException>(getPartnerId);
    }

    [Fact]
    public void MarkRead_AlreadyRead_KeepsFirstMoment()
    {
        Message message = CreateMessage();
        DateTime firstReadAt = _sentAt.AddMinutes(FirstReadMinutes);
        DateTime secondReadAt = _sentAt.AddMinutes(SecondReadMinutes);
        message.MarkRead(firstReadAt);

        message.MarkRead(secondReadAt);

        Assert.Equal(firstReadAt, message.ReadAt);
    }

    [Fact]
    public void CreateForMatch_MatchSubmission_UsesMatchChannel()
    {
        var message = Message.CreateForMatch(_matchChatSubmission, AuthorId, _sentAt);

        Assert.Equal(MessageChannel.Match, message.Channel);
    }

    [Fact]
    public void CreateForMatch_MatchSubmission_KeepsMatchId()
    {
        var message = Message.CreateForMatch(_matchChatSubmission, AuthorId, _sentAt);

        Assert.Equal(MatchId, message.MatchId);
    }

    private static Message CreateMessage()
    {
        return Message.CreatePrivate(_route, PrivateText, _sentAt);
    }
}
