using System;
using Bastion.Client.Networking.Friends;
using Bastion.Client.Networking.PrivateChat;

namespace Bastion.Client.Networking.Notifications;

public sealed class PrivateMessageReceivedEventArgs : EventArgs
{
    public PrivateMessageReceivedEventArgs(PlayerTag sender, PrivateMessage message)
    {
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentNullException.ThrowIfNull(message);

        Sender = sender;
        Message = message;
    }

    public PlayerTag Sender { get; }

    public PrivateMessage Message { get; }
}
