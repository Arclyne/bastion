namespace Bastion.Server.Application.Chat;

public interface IChatBroadcaster
{
    void Broadcast(PostedChatLine line);

    int CountListeners();
}
