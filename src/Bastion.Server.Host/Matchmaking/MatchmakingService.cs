using System;
using System.Data.Common;
using Bastion.Contracts.Matchmaking;
using Bastion.Server.Application.Matchmaking;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Host.Sessions;
using CoreWCF;
using log4net;
using Microsoft.Extensions.DependencyInjection;

namespace Bastion.Server.Host.Matchmaking;

/// <inheritdoc/>
[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
public sealed class MatchmakingService : IMatchmakingService
{
    private const string ServerFailureReason = "The server could not start the match search.";

    private static readonly ILog _logger = LogManager.GetLogger(typeof(MatchmakingService));

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;

    public MatchmakingService(IServiceScopeFactory scopeFactory, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc/>
    public void StartSearch(string? sessionToken)
    {
        if (sessionToken is null)
        {
            throw SessionTranslator.ToFault(new InvalidSessionException());
        }

        try
        {
            Run(search => search.Start(sessionToken, GetNow()));
        }
        catch (InvalidSessionException ex)
        {
            throw SessionTranslator.ToFault(ex);
        }
        catch (SearchRejectedException ex)
        {
            throw MatchmakingTranslator.ToFault(ex);
        }
        catch (DbException ex)
        {
            _logger.Error("The database could not be reached while starting a match search.", ex);
            throw new FaultException(ServerFailureReason);
        }
    }

    /// <inheritdoc/>
    public void CancelSearch(string? sessionToken)
    {
        RunOneWay(sessionToken, nameof(CancelSearch), (search, token) => search.Cancel(token, GetNow()));
    }

    /// <inheritdoc/>
    public void ConfirmPresence(string? sessionToken)
    {
        RunOneWay(sessionToken, nameof(ConfirmPresence), (search, token) => search.ConfirmPresence(token, GetNow()));
    }

    private void RunOneWay(string? sessionToken, string operationName, Action<IMatchSearchService, string> operation)
    {
        if (sessionToken is null)
        {
            _logger.Warn($"A one-way matchmaking request arrived without a session. Operation={operationName}");
            return;
        }

        // A one-way operation has no reply, so a failure is logged here instead of being sent to the client.
        try
        {
            Run(search => operation(search, sessionToken));
        }
        catch (InvalidSessionException ex)
        {
            _logger.Warn($"A one-way matchmaking request was ignored because the session is not valid. Operation={operationName}", ex);
        }
        catch (DbException ex)
        {
            _logger.Error($"The database could not be reached during a one-way matchmaking request. Operation={operationName}", ex);
        }
    }

    private void Run(Action<IMatchSearchService> operation)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IMatchSearchService search = scope.ServiceProvider.GetRequiredService<IMatchSearchService>();
        operation(search);
    }

    private DateTime GetNow()
    {
        return _timeProvider.GetUtcNow().UtcDateTime;
    }
}
