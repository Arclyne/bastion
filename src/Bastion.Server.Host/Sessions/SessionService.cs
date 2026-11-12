using System;
using System.Data.Common;
using Bastion.Contracts.Notifications;
using Bastion.Contracts.Sessions;
using Bastion.Server.Application.Sessions;
using CoreWCF;
using log4net;
using Microsoft.Extensions.DependencyInjection;

namespace Bastion.Server.Host.Sessions;

/// <inheritdoc/>
[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
public sealed class SessionService : ISessionService
{
    private const string ServerFailureReason = "The server could not open the notification channel.";

    private static readonly ILog _logger = LogManager.GetLogger(typeof(SessionService));

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly INotificationChannelRegistry _channelRegistry;
    private readonly TimeProvider _timeProvider;

    public SessionService(
        IServiceScopeFactory scopeFactory,
        INotificationChannelRegistry channelRegistry,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(channelRegistry);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _scopeFactory = scopeFactory;
        _channelRegistry = channelRegistry;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc/>
    public void KeepAlive()
    {
    }

    /// <inheritdoc/>
    public void Subscribe(string? sessionToken)
    {
        string token = SessionTranslator.RequireSessionToken(sessionToken);
        Action subscribe = () => SubscribeInRequestScope(token);
        TranslateSessionFailures(nameof(Subscribe), subscribe);
    }

    private void SubscribeInRequestScope(string token)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ISessionVerifier sessionVerifier = scope.ServiceProvider.GetRequiredService<ISessionVerifier>();
        DateTime now = _timeProvider.GetUtcNow().UtcDateTime;
        ConnectedPlayer player = sessionVerifier.Verify(token, now);
        INotificationCallback callback = OperationContext.Current.GetCallbackChannel<INotificationCallback>();
        _channelRegistry.RegisterChannel(player, callback);
    }

    private void TranslateSessionFailures(string operationName, Action operation)
    {
        try
        {
            operation();
        }
        catch (InvalidSessionException ex)
        {
            throw SessionTranslator.ToFault(ex);
        }
        catch (DbException ex)
        {
            _logger.Error($"The database could not be reached. Operation={operationName}", ex);
            throw new FaultException(ServerFailureReason);
        }
    }
}
