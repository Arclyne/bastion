using System;

namespace Bastion.Domain.Chat;

public class Message
{
    private const string SameAccountMessage = "A player cannot send a private message to themselves.";

    private Message()
    {
        Text = string.Empty;
    }

    public long MessageId { get; private set; }

    public int AuthorId { get; private set; }

    public MessageChannel Channel { get; private set; }

    public int? RecipientId { get; private set; }

    public int? MatchId { get; private set; }

    public string Text { get; private set; }

    public DateTime SentAt { get; private set; }

    public DateTime? ReadAt { get; private set; }

    public static Message CreatePrivate(MessageParticipants participants, string text, DateTime sentAt)
    {
        ArgumentNullException.ThrowIfNull(participants);
        ArgumentNullException.ThrowIfNull(text);
        if (participants.AuthorId == participants.RecipientId)
        {
            throw new ArgumentException(SameAccountMessage, nameof(participants));
        }

        return new Message
        {
            AuthorId = participants.AuthorId,
            RecipientId = participants.RecipientId,
            Channel = MessageChannel.Friends,
            Text = text,
            SentAt = sentAt,
        };
    }

    public static Message CreateForMatch(MatchChatSubmission submission, int authorId, DateTime sentAt)
    {
        ArgumentNullException.ThrowIfNull(submission);

        return new Message
        {
            AuthorId = authorId,
            Channel = MessageChannel.Match,
            MatchId = submission.MatchId,
            Text = submission.Text,
            SentAt = sentAt,
        };
    }

    public bool IsSentBy(int accountId)
    {
        return AuthorId == accountId;
    }

    public int GetPartnerId(int accountId)
    {
        if (IsSentBy(accountId))
        {
            return RecipientId.GetValueOrDefault();
        }

        return AuthorId;
    }

    public void MarkRead(DateTime readAt)
    {
        ReadAt ??= readAt;
    }
}
