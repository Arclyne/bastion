using Bastion.Client.Networking.Chat;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Networking.PrivateChat;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed record ChatClients(
    IChatClient Lobby,
    IConversationClient Conversations,
    INotificationClient Notifications);
