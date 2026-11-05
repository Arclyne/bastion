using System;

namespace Bastion.Client.Networking.Notifications;

public sealed class PresenceRequestedEventArgs : EventArgs
{
    public PresenceRequestedEventArgs(TimeSpan responseWindow)
    {
        ResponseWindow = responseWindow;
    }

    public TimeSpan ResponseWindow { get; }
}
