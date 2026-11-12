using System;

namespace Bastion.Client.Screens.Matches;

public sealed record JoinedMatchViewModels
{
    public JoinedMatchViewModels(
        MatchChatViewModel chat,
        MatchAbandonmentViewModel abandonment,
        MatchPlayViewModel play)
    {
        ArgumentNullException.ThrowIfNull(chat);
        ArgumentNullException.ThrowIfNull(abandonment);
        ArgumentNullException.ThrowIfNull(play);

        Chat = chat;
        Abandonment = abandonment;
        Play = play;
    }

    public MatchChatViewModel Chat { get; }

    public MatchAbandonmentViewModel Abandonment { get; }

    public MatchPlayViewModel Play { get; }
}
