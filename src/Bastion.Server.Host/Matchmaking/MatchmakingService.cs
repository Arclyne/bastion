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
            RunInRequestScope(searchService => searchService.StartSearch(sessionToken, GetUtcNow()));
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
        RunOneWay(sessionToken, nameof(CancelSearch), (searchService, token) => searchService.CancelSearch(token, GetUtcNow()));
    }

    /// <inheritdoc/>
    public void ConfirmPresence(string? sessionToken)
    {
        RunOneWay(sessionToken, nameof(ConfirmPresence), (searchService, token) => searchService.ConfirmPresence(token, GetUtcNow()));
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
            RunInRequestScope(searchService => operation(searchService, sessionToken));
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

    private void RunInRequestScope(Action<IMatchSearchService> operation)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IMatchSearchService searchService = scope.ServiceProvider.GetRequiredService<IMatchSearchService>();
        operation(searchService);
    }

    private DateTime GetUtcNow()
    {
        return _timeProvider.GetUtcNow().UtcDateTime;
    }
}
