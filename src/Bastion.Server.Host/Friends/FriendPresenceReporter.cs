using System;
using System.Data.Common;
using Bastion.Server.Application.Friends;
using log4net;
using Microsoft.Extensions.DependencyInjection;

namespace Bastion.Server.Host.Friends;

public sealed class FriendPresenceReporter : IFriendPresenceReporter
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
        Action reportConnected = () => ReportConnectedInOwnScope(accountId);
        LogPresenceFailures(nameof(ReportConnected), accountId, reportConnected);
    }

    public void ReportDisconnected(int accountId)
    {
        DateTime now = _timeProvider.GetUtcNow().UtcDateTime;
        Action reportDisconnected = () => ReportDisconnectedInOwnScope(accountId, now);
        LogPresenceFailures(nameof(ReportDisconnected), accountId, reportDisconnected);
    }

    private void ReportConnectedInOwnScope(int accountId)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IFriendPresenceService presenceService = scope.ServiceProvider.GetRequiredService<IFriendPresenceService>();
        presenceService.ReportConnected(accountId);
    }

    private void ReportDisconnectedInOwnScope(int accountId, DateTime now)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IFriendPresenceService presenceService = scope.ServiceProvider.GetRequiredService<IFriendPresenceService>();
        presenceService.ReportDisconnected(accountId, now);
    }

    private static void LogPresenceFailures(string operationName, int accountId, Action report)
    {
        try
        {
            report();
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
