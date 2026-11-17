namespace Bastion.Client.Screens.MainMenu.Chat;

public interface IFriendDirectory
{
    ConversationPresence GetPresence(string friendCode);
}
