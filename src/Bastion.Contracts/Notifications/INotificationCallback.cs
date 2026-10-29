using System.ServiceModel;

namespace Bastion.Contracts.Notifications;

[ServiceContract]
public interface INotificationCallback
{
    [OperationContract(IsOneWay = true)]
    void ReceiveNotification(Notification notification);
}
