namespace Bastion.Server.Host.Friends;

public interface IFriendPresenceReporter
{
    void ReportConnected(int accountId);

    void ReportDisconnected(int accountId);
}
