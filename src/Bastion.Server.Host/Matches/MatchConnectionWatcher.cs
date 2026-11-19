using System;
using System.Threading;
using System.Threading.Tasks;
using Bastion.Server.Application.Matches;
using Bastion.Server.Host.Sessions;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bastion.Server.Host.Matches;

/// <inheritdoc/>
public sealed class MatchConnectionWatcher : IHostedService
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(MatchConnectionWatcher));

    private readonly INotificationChannelRegistry _channelRegistry;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;

    public MatchConnectionWatcher(
        INotificationChannelRegistry channelRegistry,
        IServiceScopeFactory scopeFactory,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(channelRegistry);
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _channelRegistry = channelRegistry;
        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        RecoverMatchesAfterRestart();
        _channelRegistry.PlayerDisconnected += HandlePlayerDisconnected;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _channelRegistry.PlayerDisconnected -= HandlePlayerDisconnected;
        return Task.CompletedTask;
    }

    private void RecoverMatchesAfterRestart()
    {
        try
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            IMatchRecoveryService recoveryService = scope.ServiceProvider.GetRequiredService<IMatchRecoveryService>();
            recoveryService.RecoverMatchesAfterRestart(GetUtcNow());
        }
        catch (MatchRepositoryException ex)
        {
            _logger.Error("The matches in progress could not be picked up after the restart.", ex);
        }
    }

    private void HandlePlayerDisconnected(object? sender, PlayerDisconnectedEventArgs e)
    {
        try
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            IMatchConnectionService connectionService =
                scope.ServiceProvider.GetRequiredService<IMatchConnectionService>();
            connectionService.MarkDisconnected(e.AccountId, GetUtcNow());
        }
        catch (MatchRepositoryException ex)
        {
            _logger.Error($"The disconnection of a player could not be recorded. AccountId={e.AccountId}", ex);
        }
    }

    private DateTime GetUtcNow()
    {
        return _timeProvider.GetUtcNow().UtcDateTime;
    }
}
