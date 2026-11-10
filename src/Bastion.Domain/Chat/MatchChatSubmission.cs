using System;

namespace Bastion.Domain.Chat;

public sealed record MatchChatSubmission
{
    public MatchChatSubmission(int matchId, string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        MatchId = matchId;
        Text = text;
    }

    public int MatchId { get; }

    public string Text { get; }
}
