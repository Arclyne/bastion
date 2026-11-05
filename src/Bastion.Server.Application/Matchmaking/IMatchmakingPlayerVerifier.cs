using System;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Matchmaking;

public interface IMatchmakingPlayerVerifier
{
    ConnectedPlayer VerifySession(string sessionToken, DateTime now);

    ConnectedPlayer VerifyPlayerCanSearch(string sessionToken, DateTime now);
}
