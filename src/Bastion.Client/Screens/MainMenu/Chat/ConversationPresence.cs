using System;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed record ConversationPresence
{
    public ConversationPresence(bool isConnected, string statusText)
    {
        ArgumentNullException.ThrowIfNull(statusText);

        IsConnected = isConnected;
        StatusText = statusText;
    }

    public bool IsConnected { get; }

    public string StatusText { get; }
}
