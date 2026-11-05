using Bastion.Client.Networking.Friends;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Screens.MainMenu.Chat;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed record SocialClients(IFriendClient Friends, ChatClients Chat, INotificationClient Notifications);
