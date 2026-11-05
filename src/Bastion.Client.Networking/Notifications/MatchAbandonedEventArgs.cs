using System;

namespace Bastion.Client.Networking.Notifications;

public sealed class MatchAbandonedEventArgs : EventArgs
{
    public MatchAbandonedEventArgs(int matchId, string abandoningNickname)
    {
        ArgumentNullException.ThrowIfNull(abandoningNickname);

        MatchId = matchId;
        AbandoningNickname = abandoningNickname;
    }

    public int MatchId { get; }

    public string AbandoningNickname { get; }
}
