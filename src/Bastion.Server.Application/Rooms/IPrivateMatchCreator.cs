using System;
using Bastion.Server.Application.Matchmaking;

namespace Bastion.Server.Application.Rooms;

public interface IPrivateMatchCreator
{
    CreatedMatch CreateMatch(RoomSnapshot room, DateTime startedAt);
}
