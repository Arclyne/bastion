using System;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Rooms;

public interface IRoomPlayerVerifier
{
    ConnectedPlayer VerifySession(string sessionToken, DateTime now);

    ConnectedPlayer VerifyPlayerCanEnterRoom(string sessionToken, DateTime now);
}
