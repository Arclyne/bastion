namespace Bastion.Server.Application.Common;

public interface IPlayerActivity
{
    bool HasNotificationChannel(int accountId);

    bool IsSearchingMatch(int accountId);

    bool IsInRoom(int accountId);
}
