using System;
using System.Data.Common;
using Bastion.Contracts.Matches;
using Bastion.Server.Application.Matches;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Host.Sessions;
using CoreWCF;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using DomainAbandonmentPreview = Bastion.Domain.Matches.AbandonmentPreview;

namespace Bastion.Server.Host.Matches;

/// <inheritdoc/>
[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
public sealed class MatchService : IMatchService
{
    private const string ServerFailureReason = "The server could not record the match change.";

    private static readonly ILog _logger = LogManager.GetLogger(typeof(MatchService));

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;
    private readonly IMatchGate _matchGate;

    public MatchService(IServiceScopeFactory scopeFactory, TimeProvider timeProvider, IMatchGate matchGate)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(matchGate);

        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
        _matchGate = matchGate;
    }

    /// <inheritdoc/>
    public void AbandonMatch(string? sessionToken, int matchId)
    {
        string token = SessionTranslator.RequireSessionToken(sessionToken);
        Action abandonMatch = () => AbandonMatchInRequestScope(token, matchId);
        TranslateMatchFailures(nameof(AbandonMatch), matchId, abandonMatch);
    }

    /// <inheritdoc/>
    public AbandonmentPreview PreviewAbandonment(string? sessionToken, int matchId)
    {
        string token = SessionTranslator.RequireSessionToken(sessionToken);
        Func<AbandonmentPreview> previewAbandonment = () => PreviewAbandonmentInRequestScope(token, matchId);
        return TranslateMatchFailures(nameof(PreviewAbandonment), matchId, previewAbandonment);
    }

    /// <inheritdoc/>
    public void MovePawn(string? sessionToken, PawnMove? pawnMove)
    {
        string token = SessionTranslator.RequireSessionToken(sessionToken);
        if (pawnMove is null)
        {
            _logger.Warn("A pawn move arrived without its data.");
            var missingMove = new IllegalMoveException();
            throw MatchTranslator.ToFault(missingMove);
        }

        var chosenMove = MatchTranslator.ToChosenPawnMove(pawnMove);
        Action movePawn = () => MovePawnInRequestScope(token, chosenMove);
        TranslateMatchFailures(nameof(MovePawn), pawnMove.MatchId, movePawn);
    }

    /// <inheritdoc/>
    public void PlaceWall(string? sessionToken, WallPlacement? wall)
    {
        string token = SessionTranslator.RequireSessionToken(sessionToken);
        if (wall is null || !Enum.IsDefined(wall.Orientation))
        {
            _logger.Warn("A wall arrived without its data or with an unknown orientation.");
            throw MatchTranslator.CreateUnreadableWallFault();
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
        abandonmentService.AbandonMatch(token, matchId, GetUtcNow());
    }

    private AbandonmentPreview PreviewAbandonmentInRequestScope(string token, int matchId)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IMatchAbandonmentService abandonmentService =
            scope.ServiceProvider.GetRequiredService<IMatchAbandonmentService>();
        DomainAbandonmentPreview preview = abandonmentService.PreviewAbandonment(token, matchId, GetUtcNow());
        return MatchTranslator.ToContractPreview(preview);
    }

    private void MovePawnInRequestScope(string token, ChosenPawnMove chosenMove)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IMatchMoveService matchMoveService = scope.ServiceProvider.GetRequiredService<IMatchMoveService>();
        matchMoveService.MovePawn(token, chosenMove, GetUtcNow());
    }

    private void PlaceWallInRequestScope(string token, ChosenWall chosenWall)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IMatchMoveService matchMoveService = scope.ServiceProvider.GetRequiredService<IMatchMoveService>();
        matchMoveService.PlaceWall(token, chosenWall, GetUtcNow());
    }

    private TResult TranslateMatchFailures<TResult>(string operationName, int matchId, Func<TResult> operation)
    {
        try
        {
            return _matchGate.Run(matchId, operation);
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
            _logger.Error($"A database command failed. Operation={operationName}; MatchId={matchId}", ex);
            throw new FaultException(ServerFailureReason);
        }
    }

    private void TranslateMatchFailures(string operationName, int matchId, Action operation)
    {
        Func<bool> completeOperation = () => CompleteOperation(operation);
        TranslateMatchFailures(operationName, matchId, completeOperation);
    }

    private static bool CompleteOperation(Action operation)
    {
        operation();
        return true;
    }

    private DateTime GetUtcNow()
    {
        return _timeProvider.GetUtcNow().UtcDateTime;
    }
}
