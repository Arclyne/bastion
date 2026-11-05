using System;
using Bastion.Client.Networking.Matches;
using Bastion.Client.Networking.Notifications;

namespace Bastion.Client.Tests.Fakes;

public sealed class FakeMatchClient : IMatchClient
{
    public Exception? AbandonFailure { get; set; }

    public int? LastAbandonedMatchId { get; private set; }

    public event EventHandler<MatchAbandonedEventArgs>? MatchAbandoned;

    public void AbandonMatch(string sessionToken, int matchId)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        if (AbandonFailure is not null)
        {
            throw AbandonFailure;
        }

        LastAbandonedMatchId = matchId;
    }

    public bool HasMatchAbandonedSubscribers()
    {
        return MatchAbandoned is not null;
    }

    public void RaiseMatchAbandoned(int matchId, string abandoningNickname)
    {
        ArgumentNullException.ThrowIfNull(abandoningNickname);

        MatchAbandoned?.Invoke(this, new MatchAbandonedEventArgs(matchId, abandoningNickname));
    }
}
