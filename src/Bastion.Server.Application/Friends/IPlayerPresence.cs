namespace Bastion.Server.Application.Friends;

public interface IPlayerPresence
{
    bool IsPlayerConnected(int accountId);
}
