namespace Bastion.Client.Networking.Chat;

public interface IChatClient
{
    void SendMessage(string sessionToken, string text);
}
