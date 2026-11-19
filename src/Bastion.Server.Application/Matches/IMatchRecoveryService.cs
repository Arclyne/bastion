using System;

namespace Bastion.Server.Application.Matches;

/// <summary>
/// Picks up the matches that were in progress when the game server stopped.
/// </summary>
public interface IMatchRecoveryService
{
    /// <summary>
    /// Marks every player of the matches in progress as disconnected from the restart and starts the current turn at
    /// the restart, so the time the server was down comes off no clock.
    /// </summary>
    /// <param name="restartedAt">The UTC time the server started.</param>
    /// <exception cref="MatchRepositoryException">The matches could not be read or saved.</exception>
    void RecoverMatchesAfterRestart(DateTime restartedAt);
}
