using System;
using Bastion.Client.Networking.Chat;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Networking.PrivateChat;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed record ChatClients
{
    public ChatClients(IChatClient lobby, IConversationClient conversations, INotificationClient notifications)
    {
        ArgumentNullException.ThrowIfNull(lobby);
        ArgumentNullException.ThrowIfNull(conversations);
        ArgumentNullException.ThrowIfNull(notifications);

        Lobby = lobby;
        Conversations = conversations;
        Notifications = notifications;
    }

    public IChatClient Lobby { get; }

    public IConversationClient Conversations { get; }

    public INotificationClient Notifications { get; }
}
