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
    private readonly NotificationChannelRegistry _channelRegistry;
    private readonly TimeProvider _timeProvider;

    public SessionService(IServiceScopeFactory scopeFactory, NotificationChannelRegistry channelRegistry, TimeProvider timeProvider)
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
        if (sessionToken is null)
        {
            throw SessionTranslator.ToFault(new InvalidSessionException());
        }

        try
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            ISessionVerifier verifier = scope.ServiceProvider.GetRequiredService<ISessionVerifier>();
            ConnectedPlayer player = verifier.Verify(sessionToken, _timeProvider.GetUtcNow().UtcDateTime);
            _channelRegistry.RegisterChannel(player, OperationContext.Current.GetCallbackChannel<INotificationCallback>());
        }
        catch (InvalidSessionException ex)
        {
            throw SessionTranslator.ToFault(ex);
        }
        catch (DbException ex)
        {
            _logger.Error("The database could not be reached while subscribing.", ex);
            throw new FaultException(ServerFailureReason);
        }
    }
}
