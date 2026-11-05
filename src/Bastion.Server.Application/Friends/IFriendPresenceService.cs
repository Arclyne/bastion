using System;

namespace Bastion.Server.Application.Friends;

public interface IFriendPresenceService
{
    void ReportConnected(int accountId);

    void ReportDisconnected(int accountId, DateTime now);
}
