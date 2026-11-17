using System;
using Bastion.Client.Networking.Friends;
using Bastion.Client.Networking.PrivateChat;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed class PrivateMessageSentEventArgs : EventArgs
{
    public PrivateMessageSentEventArgs(PlayerTag partner, PrivateMessage message)
    {
        ArgumentNullException.ThrowIfNull(partner);
        ArgumentNullException.ThrowIfNull(message);

        Partner = partner;
        Message = message;
    }

    public PlayerTag Partner { get; }

    public PrivateMessage Message { get; }
}
