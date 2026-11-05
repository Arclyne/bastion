using System;
using System.Data.Common;
using Bastion.Server.Application.Friends;
using log4net;
using Microsoft.Extensions.DependencyInjection;

namespace Bastion.Server.Host.Friends;

public sealed class FriendPresenceReporter
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(FriendPresenceReporter));

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;

    public FriendPresenceReporter(IServiceScopeFactory scopeFactory, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
    }

    public void ReportConnected(int accountId)
    {
        Run(nameof(ReportConnected), accountId, presenceService => presenceService.ReportConnected(accountId));
    }

    public void ReportDisconnected(int accountId)
    {
        DateTime now = _timeProvider.GetUtcNow().UtcDateTime;
        Run(
            nameof(ReportDisconnected),
            accountId,
            presenceService => presenceService.ReportDisconnected(accountId, now));
    }

    private void Run(string operationName, int accountId, Action<IFriendPresenceService> operation)
    {
        try
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            IFriendPresenceService presenceService =
                scope.ServiceProvider.GetRequiredService<IFriendPresenceService>();
            operation(presenceService);
        }
        catch (DbException ex)
        {
            _logger.Error(
                $"The friend presence could not be reported. Operation={operationName}; AccountId={accountId}",
                ex);
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn(
                $"The friend presence was skipped at shutdown. Operation={operationName}; AccountId={accountId}",
                ex);
        }
    }
}
