using System;
using Bastion.Client.Networking.Matchmaking;

namespace Bastion.Client.Networking.Notifications;

public sealed class SearchEndedEventArgs : EventArgs
{
    public SearchEndedEventArgs(SearchEndReason reason)
    {
        Reason = reason;
    }

    public SearchEndReason Reason { get; }
}
