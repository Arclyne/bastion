using System;
using Bastion.Client.Networking.Chat;

namespace Bastion.Client.Networking.Notifications;

public sealed class ChatMessageReceivedEventArgs : EventArgs
{
    public ChatMessageReceivedEventArgs(ChatMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        Message = message;
    }

    public ChatMessage Message { get; }
}
