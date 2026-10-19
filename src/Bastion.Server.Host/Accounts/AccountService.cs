using System;
using System.Data.Common;
using CoreWCF;
using log4net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Bastion.Contracts.Accounts;
using Bastion.Domain.Auditing;
using Bastion.Domain.Sessions;
using Bastion.Server.Application.Accounts;
using Bastion.Server.Host.Common;

namespace Bastion.Server.Host.Accounts;

[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
public sealed class AccountService : IAccountService
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(AccountService));

    private readonly IServiceScopeFactory _scopeFactory;

    public AccountService(IServiceScopeFactory scopeFactory)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);

        _scopeFactory = scopeFactory;
    }

    public AccessDecision LogIn(PlayerCredentials credentials, ClientDevice device)
    {
        if (credentials is null || device is null)
        {
            return LoginTranslator.Deny(AccessDecisionCode.InvalidCredentials);
        }

        ClientConnection connection = LoginTranslator.ToConnection(device, CallerAddress.GetCurrent());
        AccessAttempt attempt = LoginTranslator.ToAttempt(credentials, connection, DateTime.UtcNow);
        try
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            ILoginService loginService = scope.ServiceProvider.GetRequiredService<ILoginService>();
            LoginOutcome outcome = loginService.LogIn(attempt, credentials.Password ?? string.Empty);
            return LoginTranslator.ToDecision(outcome);
        }
        catch (DbUpdateException ex)
        {
            _logger.Error($"The sign-in could not be saved. IpAddress={connection.IpAddress}", ex);
            return LoginTranslator.Deny(AccessDecisionCode.ServerError);
        }
        catch (DbException ex)
        {
            _logger.Error($"The database could not be reached. IpAddress={connection.IpAddress}", ex);
            return LoginTranslator.Deny(AccessDecisionCode.ServerError);
        }
    }
}
