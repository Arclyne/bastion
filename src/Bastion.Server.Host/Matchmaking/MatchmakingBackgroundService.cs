using System;
using System.Threading;
using System.Threading.Tasks;
using Bastion.Server.Application.Matchmaking;
using Microsoft.Extensions.Hosting;

namespace Bastion.Server.Host.Matchmaking;

public sealed class MatchmakingBackgroundService : BackgroundService
{
    private const int ReviewIntervalSeconds = 1;

    private static readonly TimeSpan _reviewInterval = TimeSpan.FromSeconds(ReviewIntervalSeconds);

    private readonly MatchmakingDispatcher _dispatcher;
    private readonly TimeProvider _timeProvider;

    public MatchmakingBackgroundService(MatchmakingDispatcher dispatcher, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _dispatcher = dispatcher;
        _timeProvider = timeProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_reviewInterval, _timeProvider);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            _dispatcher.Review(_timeProvider.GetUtcNow().UtcDateTime);
        }
    }
}
