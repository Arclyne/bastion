using System;
using System.Threading;
using System.Threading.Tasks;
using Bastion.Server.Application.Matches;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bastion.Server.Host.Matches;

/// <inheritdoc/>
public sealed class MatchClockBackgroundService : BackgroundService
{
    private const int ReviewIntervalSeconds = 1;

    private static readonly TimeSpan _reviewInterval = TimeSpan.FromSeconds(ReviewIntervalSeconds);
    private static readonly ILog _logger = LogManager.GetLogger(typeof(MatchClockBackgroundService));

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;

    public MatchClockBackgroundService(IServiceScopeFactory scopeFactory, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_reviewInterval, _timeProvider);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            ReviewClocks();
        }
    }

    private void ReviewClocks()
    {
        try
        {
            ExpireRunOutClocksInScope();
        }
        catch (MatchRepositoryException ex)
        {
            _logger.Error("The clocks of the matches in progress could not be reviewed; they are reviewed again.", ex);
        }
    }

    private void ExpireRunOutClocksInScope()
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IMatchClockService clockService = scope.ServiceProvider.GetRequiredService<IMatchClockService>();
        clockService.ExpireRunOutClocks(_timeProvider.GetUtcNow().UtcDateTime);
    }
}
