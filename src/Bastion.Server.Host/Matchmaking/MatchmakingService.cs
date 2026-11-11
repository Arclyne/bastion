using System;
using System.Data.Common;
using Bastion.Contracts.Matchmaking;
using Bastion.Server.Application.Matches;
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
        string token = SessionTranslator.RequireSessionToken(sessionToken);
        Action startSearch = () => StartSearchInRequestScope(token);
        TranslateMatchmakingFailures(nameof(StartSearch), startSearch);
    }

    /// <inheritdoc/>
    public void CancelSearch(string? sessionToken)
    {
        Action<string> cancelSearch = CancelSearchInRequestScope;
        LogOneWayFailures(nameof(CancelSearch), sessionToken, cancelSearch);
    }

    /// <inheritdoc/>
    public void ConfirmPresence(string? sessionToken)
    {
        Action<string> confirmPresence = ConfirmPresenceInRequestScope;
        LogOneWayFailures(nameof(ConfirmPresence), sessionToken, confirmPresence);
    }

    private void StartSearchInRequestScope(string token)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IMatchSearchService matchSearch = scope.ServiceProvider.GetRequiredService<IMatchSearchService>();
        matchSearch.StartSearch(token, GetUtcNow());
    }

    private void CancelSearchInRequestScope(string token)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IMatchSearchService matchSearch = scope.ServiceProvider.GetRequiredService<IMatchSearchService>();
        matchSearch.CancelSearch(token, GetUtcNow());
    }

    private void ConfirmPresenceInRequestScope(string token)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IMatchSearchService matchSearch = scope.ServiceProvider.GetRequiredService<IMatchSearchService>();
        matchSearch.ConfirmPresence(token, GetUtcNow());
    }

    private void TranslateMatchmakingFailures(string operationName, Action operation)
    {
        try
        {
            operation();
        }
        catch (InvalidSessionException ex)
        {
            throw SessionTranslator.ToFault(ex);
        }
        catch (SearchRejectedException ex)
        {
            throw MatchmakingTranslator.ToFault(ex);
        }
        catch (MatchRepositoryException ex)
        {
            _logger.Error($"The match repository could not be read. Operation={operationName}", ex);
            throw new FaultException(ServerFailureReason);
        }
        catch (DbException ex)
        {
            _logger.Error($"The database could not be reached. Operation={operationName}", ex);
            throw new FaultException(ServerFailureReason);
        }
    }

    private static void LogOneWayFailures(string operationName, string? sessionToken, Action<string> operation)
    {
        if (sessionToken is null)
        {
            _logger.Warn($"A one-way matchmaking request arrived without a session. Operation={operationName}");
            return;
        }

        try
        {
            operation(sessionToken);
        }
        catch (InvalidSessionException ex)
        {
            _logger.Warn(
                $"A one-way matchmaking request was ignored because the session is not valid. Operation={operationName}",
                ex);
        }
        catch (DbException ex)
        {
            _logger.Error(
                $"The database could not be reached during a one-way matchmaking request. Operation={operationName}",
                ex);
        }
    }

    private DateTime GetUtcNow()
    {
        return _timeProvider.GetUtcNow().UtcDateTime;
    }
}
