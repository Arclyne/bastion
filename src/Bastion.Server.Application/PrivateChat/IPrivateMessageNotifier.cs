namespace Bastion.Server.Application.PrivateChat;

public interface IPrivateMessageNotifier
{
    void NotifyPrivateMessage(PrivateMessageDelivery delivery);
}
