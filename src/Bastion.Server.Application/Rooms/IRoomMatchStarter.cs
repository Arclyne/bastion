using System;
using Bastion.Server.Application.Matchmaking;

namespace Bastion.Server.Application.Rooms;

public interface IRoomMatchStarter
{
    CreatedMatch StartMatch(int hostAccountId, DateTime now);
}
