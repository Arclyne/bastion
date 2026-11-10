using System;
using System.Collections.Generic;
using Bastion.Server.Application.Accounts;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeSessionVerifier : ISessionVerifier
{
    private readonly Dictionary<string, ConnectedPlayer> _players = new Dictionary<string, ConnectedPlayer>();
    private readonly Dictionary<string, PlayerIdentity> _identities = new Dictionary<string, PlayerIdentity>();

    public void Add(string sessionToken, ConnectedPlayer player)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(player);

        _players[sessionToken] = player;
    }

    public void Add(string sessionToken, PlayerIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(identity);

        _players[sessionToken] = new ConnectedPlayer(identity.AccountId, identity.Nickname);
        _identities[sessionToken] = identity;
    }

    public ConnectedPlayer Verify(string sessionToken, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        if (!_players.TryGetValue(sessionToken, out ConnectedPlayer? player))
        {
            throw new InvalidSessionException();
        }

        return player;
    }

    public PlayerIdentity VerifyIdentity(string sessionToken, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        if (!_identities.TryGetValue(sessionToken, out PlayerIdentity? identity))
        {
            throw new InvalidSessionException();
        }

        return identity;
    }
}
