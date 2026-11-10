using System;
using Bastion.Server.Application.Accounts;

namespace Bastion.Server.Application.Sessions;

public interface ISessionVerifier
{
    ConnectedPlayer Verify(string sessionToken, DateTime now);

    PlayerIdentity VerifyIdentity(string sessionToken, DateTime now);
}
