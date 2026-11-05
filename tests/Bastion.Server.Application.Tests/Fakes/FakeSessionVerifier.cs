using System;
using System.Collections.Generic;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeSessionVerifier : ISessionVerifier
{
    private readonly Dictionary<string, ConnectedPlayer> _players = new Dictionary<string, ConnectedPlayer>();

    public void Add(string sessionToken, ConnectedPlayer player)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(player);

        _players[sessionToken] = player;
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
}
