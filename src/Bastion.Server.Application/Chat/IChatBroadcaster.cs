namespace Bastion.Server.Application.Chat;

public interface IChatBroadcaster
{
    void Broadcast(ChatLine line);
}
