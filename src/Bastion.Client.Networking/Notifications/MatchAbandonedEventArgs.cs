using System;

namespace Bastion.Client.Networking.Notifications;

public sealed class MatchAbandonedEventArgs : EventArgs
{
    public MatchAbandonedEventArgs(int matchId, string abandoningNickname, bool isMatchOver)
    {
        ArgumentNullException.ThrowIfNull(abandoningNickname);

        MatchId = matchId;
        AbandoningNickname = abandoningNickname;
        IsMatchOver = isMatchOver;
    }

    public int MatchId { get; }

    public string AbandoningNickname { get; }

    public bool IsMatchOver { get; }
}
