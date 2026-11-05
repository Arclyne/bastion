using System;

namespace Bastion.Domain.Chat;

public class ChatMessage
{
    private ChatMessage()
    {
        Text = string.Empty;
    }

    public long MessageId { get; private set; }

    public int AuthorId { get; private set; }

    public ChatChannel Channel { get; private set; }

    public int? MatchId { get; private set; }

    public int? RoomId { get; private set; }

    public string Text { get; private set; }

    public DateTime SentAt { get; private set; }

    public static ChatMessage WriteInMatch(MatchChatSubmission submission, int authorId, DateTime sentAt)
    {
        ArgumentNullException.ThrowIfNull(submission);

        return new ChatMessage
        {
            AuthorId = authorId,
            Channel = ChatChannel.Match,
            MatchId = submission.MatchId,
            Text = submission.Text.Trim(),
            SentAt = sentAt,
        };
    }
}
