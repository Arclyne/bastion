using System;

namespace Bastion.Server.Application.Matchmaking;

public interface IMatchmakingDispatcher
{
    void ReviewAndNotify(DateTime now);
}
