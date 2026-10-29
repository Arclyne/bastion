using System;
using System.Data.Common;
using CoreWCF;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using Bastion.Contracts.Notifications;
using Bastion.Contracts.Sessions;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Host.Sessions;

[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
public sealed class SessionService : ISessionService
{
    private const string ServerFailureReason = "The server could not open the notification channel.";

    private static readonly ILog _logger = LogManager.GetLogger(typeof(SessionService));

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ConnectedPlayerRegistry _registry;

    public SessionService(IServiceScopeFactory scopeFactory, ConnectedPlayerRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(registry);

        _scopeFactory = scopeFactory;
        _registry = registry;
    }

    public void Subscribe(string sessionToken)
    {
        try
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            ISessionVerifier verifier = scope.ServiceProvider.GetRequiredService<ISessionVerifier>();
            ConnectedPlayer player = verifier.Verify(sessionToken ?? string.Empty, DateTime.UtcNow);
            _registry.Register(player, OperationContext.Current.GetCallbackChannel<INotificationCallback>());
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
