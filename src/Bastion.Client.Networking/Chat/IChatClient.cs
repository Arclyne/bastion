namespace Bastion.Client.Networking.Chat;

public interface IChatClient
{
    void Send(string sessionToken, string text);
}
