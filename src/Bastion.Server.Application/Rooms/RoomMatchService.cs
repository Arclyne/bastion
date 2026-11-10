using System;
using Bastion.Server.Application.Matchmaking;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Rooms;

/// <inheritdoc/>
public sealed class RoomMatchService : IRoomMatchService
{
    private readonly IRoomPlayerVerifier _playerVerifier;
    private readonly IRoomMatchStarter _matchStarter;

    public RoomMatchService(IRoomPlayerVerifier playerVerifier, IRoomMatchStarter matchStarter)
    {
        ArgumentNullException.ThrowIfNull(playerVerifier);
        ArgumentNullException.ThrowIfNull(matchStarter);

        _playerVerifier = playerVerifier;
        _matchStarter = matchStarter;
    }

    /// <inheritdoc/>
    public int StartMatch(string sessionToken, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        ConnectedPlayer host = _playerVerifier.VerifySession(sessionToken, now);
        CreatedMatch createdMatch = _matchStarter.StartMatch(host.AccountId, now);
        return createdMatch.MatchId;
    }
}
