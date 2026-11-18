using System;

namespace Bastion.Client.Screens.Matches;

public sealed record JoinedMatchViewModelSet
{
    public JoinedMatchViewModelSet(
        MatchChatViewModel chat,
        MatchAbandonmentViewModel abandonment,
        MatchPlayViewModel matchPlayViewModel)
    {
        ArgumentNullException.ThrowIfNull(chat);
        ArgumentNullException.ThrowIfNull(abandonment);
        ArgumentNullException.ThrowIfNull(matchPlayViewModel);

        Chat = chat;
        Abandonment = abandonment;
        MatchPlay = matchPlayViewModel;
    }

    public MatchChatViewModel Chat { get; }

    public MatchAbandonmentViewModel Abandonment { get; }

    public MatchPlayViewModel MatchPlay { get; }
}
