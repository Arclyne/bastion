using System;
using System.Data.Common;
using Bastion.Contracts.Rooms;
using Bastion.Server.Application.Matches;
using Bastion.Server.Application.Rooms;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Host.Sessions;
using CoreWCF;
using log4net;
using Microsoft.Extensions.DependencyInjection;

namespace Bastion.Server.Host.Rooms;

/// <inheritdoc/>
[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
public sealed class RoomService : IRoomService
{
    private const string ServerFailureReason = "The server could not complete the room request.";

    private static readonly ILog _logger = LogManager.GetLogger(typeof(RoomService));

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;

    public RoomService(IServiceScopeFactory scopeFactory, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc/>
    public RoomState CreateRoom(string? sessionToken, RoomOptions? options)
    {
        string token = RequireSession(sessionToken);
        RequestedRoomSettings requestedSettings = RequireSettings(options, nameof(CreateRoom));
        return RunRequest(
            nameof(CreateRoom),
            roomService => roomService.CreateRoom(token, requestedSettings, GetUtcNow()));
    }

    /// <inheritdoc/>
    public RoomState JoinRoom(string? sessionToken, string? code)
    {
        string token = RequireSession(sessionToken);
        string roomCode = code ?? string.Empty;
        return RunRequest(nameof(JoinRoom), roomService => roomService.JoinRoom(token, roomCode, GetUtcNow()));
    }

    /// <inheritdoc/>
    public RoomState SetReady(string? sessionToken, bool isReady)
    {
        string token = RequireSession(sessionToken);
        return RunRequest(nameof(SetReady), roomService => roomService.SetReady(token, isReady, GetUtcNow()));
    }

    /// <inheritdoc/>
    public RoomState ChangeOptions(string? sessionToken, RoomOptions? options)
    {
        string token = RequireSession(sessionToken);
        RequestedRoomSettings requestedSettings = RequireSettings(options, nameof(ChangeOptions));
        return RunRequest(
            nameof(ChangeOptions),
            roomService => roomService.ChangeSettings(token, requestedSettings, GetUtcNow()));
    }

    /// <inheritdoc/>
    public void StartMatch(string? sessionToken)
    {
        string token = RequireSession(sessionToken);
        RunGuarded<IRoomMatchService, int>(
            nameof(StartMatch),
            matchService => matchService.StartMatch(token, GetUtcNow()));
    }

    /// <inheritdoc/>
    public void LeaveRoom(string? sessionToken)
    {
        if (sessionToken is null)
        {
            _logger.Warn("A request to leave a room arrived without a session.");
            return;
        }

        try
        {
            RunInRequestScope(roomService => roomService.LeaveRoom(sessionToken, GetUtcNow()));
        }
        catch (InvalidSessionException ex)
        {
            _logger.Warn("A request to leave a room was ignored because the session is not valid.", ex);
        }
        catch (RoomRepositoryException ex)
        {
            _logger.Error("The closed room could not be saved.", ex);
        }
    }

    /// <inheritdoc/>
    public RoomState? FindOwnRoom(string? sessionToken)
    {
        string token = RequireSession(sessionToken);
        try
        {
            RoomSnapshot? room = RunInRequestScope<IPrivateRoomService, RoomSnapshot?>(
                roomService => roomService.FindOwnRoom(token, GetUtcNow()));
            return room is null ? null : RoomTranslator.ToState(room);
        }
        catch (InvalidSessionException ex)
        {
            throw SessionTranslator.ToFault(ex);
        }
    }

    private static string RequireSession(string? sessionToken)
    {
        if (sessionToken is null)
        {
            throw SessionTranslator.ToFault(new InvalidSessionException());
        }

        return sessionToken;
    }

    private static RequestedRoomSettings RequireSettings(RoomOptions? options, string operationName)
    {
        if (options is null)
        {
            _logger.Warn($"A room request arrived without settings. Operation={operationName}");
            throw RoomTranslator.ToFault(new RoomRejectedException(RoomRejection.InvalidSettings));
        }

        return RoomTranslator.ToRequestedSettings(options);
    }

    private RoomState RunRequest(string operationName, Func<IPrivateRoomService, RoomSnapshot> operation)
    {
        return RoomTranslator.ToState(RunGuarded(operationName, operation));
    }

    private TResult RunGuarded<TService, TResult>(string operationName, Func<TService, TResult> operation)
        where TService : notnull
    {
        try
        {
            return RunInRequestScope(operation);
        }
        catch (InvalidSessionException ex)
        {
            throw SessionTranslator.ToFault(ex);
        }
        catch (RoomRejectedException ex)
        {
            LogRejection(operationName, ex);
            throw RoomTranslator.ToFault(ex);
        }
        catch (RoomRepositoryException ex)
        {
            _logger.Error($"The room repository failed during a room request. Operation={operationName}", ex);
            throw new FaultException(ServerFailureReason);
        }
        catch (MatchRepositoryException ex)
        {
            _logger.Error($"The match repository failed during a room request. Operation={operationName}", ex);
            throw new FaultException(ServerFailureReason);
        }
        catch (DbException ex)
        {
            _logger.Error($"The database could not be reached during a room request. Operation={operationName}", ex);
            throw new FaultException(ServerFailureReason);
        }
    }

    private static void LogRejection(string operationName, RoomRejectedException rejection)
    {
        if (rejection.Rejection == RoomRejection.InvalidSettings)
        {
            _logger.Warn($"A room request carried settings the client never offers. Operation={operationName}");
            return;
        }

        _logger.Info($"A room request was rejected. Operation={operationName}; Rejection={rejection.Rejection}");
    }

    private void RunInRequestScope(Action<IPrivateRoomService> operation)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IPrivateRoomService roomService = scope.ServiceProvider.GetRequiredService<IPrivateRoomService>();
        operation(roomService);
    }

    private TResult RunInRequestScope<TService, TResult>(Func<TService, TResult> operation)
        where TService : notnull
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        TService service = scope.ServiceProvider.GetRequiredService<TService>();
        return operation(service);
    }

    private DateTime GetUtcNow()
    {
        return _timeProvider.GetUtcNow().UtcDateTime;
    }
}
