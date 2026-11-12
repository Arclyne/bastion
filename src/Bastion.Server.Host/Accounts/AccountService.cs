using System;
using System.Data.Common;
using Bastion.Contracts.Accounts;
using Bastion.Domain.Auditing;
using Bastion.Domain.Sessions;
using Bastion.Server.Application.Accounts;
using Bastion.Server.Host.Common;
using CoreWCF;
using log4net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bastion.Server.Host.Accounts;

/// <inheritdoc/>
[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
public sealed class AccountService : IAccountService
{
    private const string ServerFailureReason = "The server could not complete the login.";

    private static readonly ILog _logger = LogManager.GetLogger(typeof(AccountService));

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;

    public AccountService(IServiceScopeFactory scopeFactory, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc/>
    public PlayerSession LogIn(PlayerCredentials credentials, ClientDevice device)
    {
        if (credentials is null || device is null)
        {
            throw LoginTranslator.ToFault(new InvalidCredentialsException());
        }

        ClientConnection connection = LoginTranslator.ToConnection(device, CallerAddress.GetCurrent());
        AccessAttempt attempt = LoginTranslator.ToAttempt(
            credentials,
            connection,
            _timeProvider.GetUtcNow().UtcDateTime);
        try
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            ILoginService loginService = scope.ServiceProvider.GetRequiredService<ILoginService>();
            LoggedInPlayer player = loginService.LogIn(attempt, credentials.Password ?? string.Empty);
            return LoginTranslator.ToSession(player);
        }
        catch (InvalidCredentialsException ex)
        {
            throw LoginTranslator.ToFault(ex);
        }
        catch (AccountRestrictedException ex)
        {
            throw LoginTranslator.ToFault(ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.Error($"The login could not be saved. IpAddress={connection.IpAddress}", ex);
            throw new FaultException(ServerFailureReason);
        }
        catch (DbException ex)
        {
            _logger.Error($"The database could not be reached. IpAddress={connection.IpAddress}", ex);
            throw new FaultException(ServerFailureReason);
        }
    }
}
