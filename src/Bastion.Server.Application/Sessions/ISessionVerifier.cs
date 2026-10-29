using System;

namespace Bastion.Server.Application.Sessions;

public interface ISessionVerifier
{
    ConnectedPlayer Verify(string sessionToken, DateTime now);
}
