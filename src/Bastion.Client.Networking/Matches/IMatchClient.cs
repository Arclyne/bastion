using System;
using Bastion.Client.Networking.Notifications;

namespace Bastion.Client.Networking.Matches;

public interface IMatchClient
{
    event EventHandler<MatchAbandonedEventArgs>? MatchAbandoned;

    void AbandonMatch(string sessionToken, int matchId);
}
