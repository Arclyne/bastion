using System;
using System.Data.Common;
using Bastion.Contracts.Rooms;
using Bastion.Domain.Common;
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
        string token = SessionTranslator.RequireSessionToken(sessionToken);
        RequestedRoomSettings requestedSettings = RequireSettings(options, nameof(CreateRoom));
        Func<RoomState> createRoom = () => CreateRoomInRequestScope(token, requestedSettings);
        return TranslateRoomFailures(nameof(CreateRoom), createRoom);
    }

    /// <inheritdoc/>
    public RoomState JoinRoom(string? sessionToken, string? code)
    {
        string token = SessionTranslator.RequireSessionToken(sessionToken);
        string roomCode = code ?? string.Empty;
        Func<RoomState> joinRoom = () => JoinRoomInRequestScope(token, roomCode);
        return TranslateRoomFailures(nameof(JoinRoom), joinRoom);
    }

    /// <inheritdoc/>
    public RoomState SetReady(string? sessionToken, bool isReady)
    {
        string token = SessionTranslator.RequireSessionToken(sessionToken);
        Func<RoomState> setReady = () => SetReadyInRequestScope(token, isReady);
        return TranslateRoomFailures(nameof(SetReady), setReady);
    }

    /// <inheritdoc/>
    public RoomState ChangeOptions(string? sessionToken, RoomOptions? options)
    {
        string token = SessionTranslator.RequireSessionToken(sessionToken);
        RequestedRoomSettings requestedSettings = RequireSettings(options, nameof(ChangeOptions));
        Func<RoomState> changeOptions = () => ChangeOptionsInRequestScope(token, requestedSettings);
        return TranslateRoomFailures(nameof(ChangeOptions), changeOptions);
    }

    /// <inheritdoc/>
    public void StartMatch(string? sessionToken)
    {
        string token = SessionTranslator.RequireSessionToken(sessionToken);
        Action startMatch = () => StartMatchInRequestScope(token);
        TranslateRoomFailures(nameof(StartMatch), startMatch);
    }

    /// <inheritdoc/>
    public RoomState KickPlayer(string? sessionToken, string? nickname)
    {
        string token = SessionTranslator.RequireSessionToken(sessionToken);
        string kickedNickname = nickname ?? string.Empty;
        Func<RoomState> kickPlayer = () => KickPlayerInRequestScope(token, kickedNickname);
        return TranslateRoomFailures(nameof(KickPlayer), kickPlayer);
    }

    /// <inheritdoc/>
    public void LeaveRoom(string? sessionToken)
    {
        Action<string> leaveRoom = LeaveRoomInRequestScope;
        LogOneWayFailures(nameof(LeaveRoom), sessionToken, leaveRoom);
    }

    /// <inheritdoc/>
    public OwnRoomResult FindOwnRoom(string? sessionToken)
    {
        string token = SessionTranslator.RequireSessionToken(sessionToken);
        Func<OwnRoomResult> findOwnRoom = () => FindOwnRoomInRequestScope(token);
        return TranslateRoomFailures(nameof(FindOwnRoom), findOwnRoom);
    }

    private static RequestedRoomSettings RequireSettings(RoomOptions? options, string operationName)
    {
        if (options is null || !Enum.IsDefined(options.GameMode))
        {
            _logger.Warn($"A room request arrived without valid settings. Operation={operationName}");
            var invalidSettings = new RoomRejectedException(RoomRejection.InvalidSettings);
            throw RoomTranslator.ToFault(invalidSettings);
        }

        return RoomTranslator.ToRequestedSettings(options);
    }

    private RoomState CreateRoomInRequestScope(string token, RequestedRoomSettings requestedSettings)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IPrivateRoomService privateRoomService = scope.ServiceProvider.GetRequiredService<IPrivateRoomService>();
        RoomSnapshot room = privateRoomService.CreateRoom(token, requestedSettings, GetUtcNow());
        return RoomTranslator.ToState(room);
    }

    private RoomState JoinRoomInRequestScope(string token, string roomCode)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IPrivateRoomService privateRoomService = scope.ServiceProvider.GetRequiredService<IPrivateRoomService>();
        RoomSnapshot room = privateRoomService.JoinRoom(token, roomCode, GetUtcNow());
        return RoomTranslator.ToState(room);
    }

    private RoomState SetReadyInRequestScope(string token, bool isReady)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IPrivateRoomService privateRoomService = scope.ServiceProvider.GetRequiredService<IPrivateRoomService>();
        RoomSnapshot room = privateRoomService.SetReady(token, isReady, GetUtcNow());
        return RoomTranslator.ToState(room);
    }

    private RoomState ChangeOptionsInRequestScope(string token, RequestedRoomSettings requestedSettings)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IPrivateRoomService privateRoomService = scope.ServiceProvider.GetRequiredService<IPrivateRoomService>();
        RoomSnapshot room = privateRoomService.ChangeSettings(token, requestedSettings, GetUtcNow());
        return RoomTranslator.ToState(room);
    }

    private void StartMatchInRequestScope(string token)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IRoomMatchService roomMatchService = scope.ServiceProvider.GetRequiredService<IRoomMatchService>();
        roomMatchService.StartMatch(token, GetUtcNow());
    }

    private RoomState KickPlayerInRequestScope(string token, string kickedNickname)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IPrivateRoomService privateRoomService = scope.ServiceProvider.GetRequiredService<IPrivateRoomService>();
        RoomSnapshot room = privateRoomService.KickPlayer(token, kickedNickname, GetUtcNow());
        _logger.Info($"A host kicked a player from their room. RoomId={room.Identity.RoomId}");
        return RoomTranslator.ToState(room);
    }

    private void LeaveRoomInRequestScope(string token)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IPrivateRoomService privateRoomService = scope.ServiceProvider.GetRequiredService<IPrivateRoomService>();
        privateRoomService.LeaveRoom(token, GetUtcNow());
    }

    private static void LogOneWayFailures(string operationName, string? sessionToken, Action<string> operation)
    {
        if (sessionToken is null)
        {
            _logger.Warn($"A one-way room request arrived without a session. Operation={operationName}");
            return;
        }

        try
        {
            operation(sessionToken);
        }
        catch (InvalidSessionException ex)
        {
            _logger.Warn(
                $"A one-way room request was ignored because the session is not valid. Operation={operationName}",
                ex);
        }
        catch (RoomRepositoryException ex)
        {
            _logger.Error($"The closed room could not be saved. Operation={operationName}", ex);
        }
        catch (DbException ex)
        {
            _logger.Error($"A database command failed during a one-way room request. Operation={operationName}", ex);
        }
    }

    private OwnRoomResult FindOwnRoomInRequestScope(string token)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IPrivateRoomService privateRoomService = scope.ServiceProvider.GetRequiredService<IPrivateRoomService>();
        Lookup<RoomSnapshot> foundRoom = privateRoomService.FindOwnRoom(token, GetUtcNow());
        return RoomTranslator.ToOwnRoomResult(foundRoom);
    }

    private static TResult TranslateRoomFailures<TResult>(string operationName, Func<TResult> operation)
    {
        try
        {
            return operation();
        }
        catch (InvalidSessionException ex)
        {
            throw SessionTranslator.ToFault(ex);
        }
        catch (RoomRejectedException ex)
        {
            throw RoomTranslator.ToFault(ex);
        }
        catch (RoomCodeUnavailableException ex)
        {
            _logger.Error($"No free room code was found during a room request. Operation={operationName}", ex);
            throw new FaultException(ServerFailureReason);
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
            _logger.Error($"A database command failed during a room request. Operation={operationName}", ex);
            throw new FaultException(ServerFailureReason);
        }
    }

    private static void TranslateRoomFailures(string operationName, Action operation)
    {
        Func<bool> completeOperation = () => CompleteOperation(operation);
        TranslateRoomFailures(operationName, completeOperation);
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
