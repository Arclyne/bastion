namespace Bastion.Client.Screens.MainMenu.Chat;

public interface IFriendDirectory
{
    ConversationPresence? FindPresence(string friendCode);
}
