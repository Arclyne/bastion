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
    private const string ServerFailureReason = "The server could not record the match change.";

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
        string token = SessionTranslator.RequireSessionToken(sessionToken);
        Action abandonMatch = () => AbandonMatchInRequestScope(token, matchId);
        TranslateMatchFailures(nameof(AbandonMatch), matchId, abandonMatch);
    }

    /// <inheritdoc/>
    public void MovePawn(string? sessionToken, PawnMove? move)
    {
        string token = SessionTranslator.RequireSessionToken(sessionToken);
        if (move is null)
        {
            _logger.Warn("A pawn move arrived without its data.");
            var missingMove = new IllegalMoveException();
            throw MatchTranslator.ToFault(missingMove);
        }

        var chosenMove = MatchTranslator.ToChosenPawnMove(move);
        Action movePawn = () => MovePawnInRequestScope(token, chosenMove);
        TranslateMatchFailures(nameof(MovePawn), move.MatchId, movePawn);
    }

    /// <inheritdoc/>
    public void PlaceWall(string? sessionToken, WallPlacement? wall)
    {
        string token = SessionTranslator.RequireSessionToken(sessionToken);
        if (wall is null || !Enum.IsDefined(wall.Orientation))
        {
            _logger.Warn("A wall arrived without its data or with an unknown orientation.");
            throw MatchTranslator.ToUnreadableWallFault();
        }

        var chosenWall = MatchTranslator.ToChosenWall(wall);
        Action placeWall = () => PlaceWallInRequestScope(token, chosenWall);
        TranslateMatchFailures(nameof(PlaceWall), wall.MatchId, placeWall);
    }

    private void AbandonMatchInRequestScope(string token, int matchId)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IMatchAbandonmentService abandonmentService =
            scope.ServiceProvider.GetRequiredService<IMatchAbandonmentService>();
        DateTime now = _timeProvider.GetUtcNow().UtcDateTime;
        abandonmentService.AbandonMatch(token, matchId, now);
    }

    private void MovePawnInRequestScope(string token, ChosenPawnMove move)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IMatchMoveService matchMoveService = scope.ServiceProvider.GetRequiredService<IMatchMoveService>();
        DateTime now = _timeProvider.GetUtcNow().UtcDateTime;
        matchMoveService.MovePawn(token, move, now);
    }

    private void PlaceWallInRequestScope(string token, ChosenWall chosenWall)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IMatchMoveService matchMoveService = scope.ServiceProvider.GetRequiredService<IMatchMoveService>();
        DateTime now = _timeProvider.GetUtcNow().UtcDateTime;
        matchMoveService.PlaceWall(token, chosenWall, now);
    }

    private void TranslateMatchFailures(string operationName, int matchId, Action operation)
    {
        try
        {
            operation();
        }
        catch (InvalidSessionException ex)
        {
            throw SessionTranslator.ToFault(ex);
        }
        catch (MatchNotJoinedException ex)
        {
            throw MatchTranslator.ToFault(ex);
        }
        catch (NotYourTurnException ex)
        {
            throw MatchTranslator.ToFault(ex);
        }
        catch (IllegalMoveException ex)
        {
            throw MatchTranslator.ToFault(ex);
        }
        catch (IllegalWallException ex)
        {
            throw MatchTranslator.ToFault(ex);
        }
        catch (MatchRepositoryException ex)
        {
            _logger.Error($"The match repository could not be used. Operation={operationName}; MatchId={matchId}", ex);
            throw new FaultException(ServerFailureReason);
        }
        catch (DbException ex)
        {
            _logger.Error($"The database could not be reached. Operation={operationName}; MatchId={matchId}", ex);
            throw new FaultException(ServerFailureReason);
        }
    }
}
