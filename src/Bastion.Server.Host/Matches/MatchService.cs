using System;
using System.Data.Common;
using Bastion.Contracts.Matches;
using Bastion.Server.Application.Matches;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Host.Sessions;
using CoreWCF;
using log4net;
using Microsoft.Extensions.DependencyInjection;

namespace Bastion.Server.Host.Matches;

/// <inheritdoc/>
[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
public sealed class MatchService : IMatchService
{
    private const string ServerFailureReason = "The server could not close the match.";

    private static readonly ILog _logger = LogManager.GetLogger(typeof(MatchService));

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;

    public MatchService(IServiceScopeFactory scopeFactory, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc/>
    public void AbandonMatch(string? sessionToken, int matchId)
    {
        if (sessionToken is null)
        {
            throw SessionTranslator.ToFault(new InvalidSessionException());
        }

        try
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            IMatchAbandonmentService abandonmentService =
                scope.ServiceProvider.GetRequiredService<IMatchAbandonmentService>();
            abandonmentService.AbandonMatch(sessionToken, matchId, _timeProvider.GetUtcNow().UtcDateTime);
        }
        catch (InvalidSessionException ex)
        {
            throw SessionTranslator.ToFault(ex);
        }
        catch (MatchNotJoinedException ex)
        {
            throw MatchTranslator.ToFault(ex);
        }
        catch (MatchStoreException ex)
        {
            _logger.Error($"The match store failed while a player left a match. MatchId={matchId}", ex);
            throw new FaultException(ServerFailureReason);
        }
        catch (DbException ex)
        {
            _logger.Error($"The database could not be reached while a player left a match. MatchId={matchId}", ex);
            throw new FaultException(ServerFailureReason);
        }
    }
}
