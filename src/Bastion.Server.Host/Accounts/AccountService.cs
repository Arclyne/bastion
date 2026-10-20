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
    private const string ServerFailureReason = "The server could not complete the sign-in.";

    private static readonly ILog _logger = LogManager.GetLogger(typeof(AccountService));

    private readonly IServiceScopeFactory _scopeFactory;

    public AccountService(IServiceScopeFactory scopeFactory)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);

        _scopeFactory = scopeFactory;
    }

    public PlayerSession LogIn(PlayerCredentials credentials, ClientDevice device)
    {
        if (credentials is null || device is null)
        {
            throw LoginTranslator.ToFault(new InvalidCredentialsException());
        }

        ClientConnection connection = LoginTranslator.ToConnection(device, CallerAddress.GetCurrent());
        AccessAttempt attempt = LoginTranslator.ToAttempt(credentials, connection, DateTime.UtcNow);
        try
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            ILoginService loginService = scope.ServiceProvider.GetRequiredService<ILoginService>();
            SignedInPlayer player = loginService.LogIn(attempt, credentials.Password ?? string.Empty);
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
            _logger.Error($"The sign-in could not be saved. IpAddress={connection.IpAddress}", ex);
            throw new FaultException(ServerFailureReason);
        }
        catch (DbException ex)
        {
            _logger.Error($"The database could not be reached. IpAddress={connection.IpAddress}", ex);
            throw new FaultException(ServerFailureReason);
        }
    }
}
