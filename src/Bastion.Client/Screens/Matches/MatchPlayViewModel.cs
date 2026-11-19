using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Matches;
using Bastion.Client.Networking.Matchmaking;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Networking.Sessions;
using Bastion.Domain.Matches;
using Gum.Mvvm;
using log4net;

namespace Bastion.Client.Screens.Matches;

public sealed class MatchPlayViewModel : ViewModel, IDisposable
{
    public const int MoveNotificationWaitSeconds = 5;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(MatchPlayViewModel));
    private static readonly TimeSpan _moveNotificationWait = TimeSpan.FromSeconds(MoveNotificationWaitSeconds);

    private readonly JoinedMatch _joinedMatch;
    private readonly IMatchClient _matches;
    private readonly TimeProvider _timeProvider;
    private readonly SynchronizationContext? _uiContext;
    private readonly Dictionary<byte, BoardCoordinates> _pawnsByTurnOrder;
    private readonly Dictionary<string, byte> _turnOrdersByNickname;
    private readonly Dictionary<byte, string> _nicknamesByTurnOrder;
    private readonly Dictionary<byte, byte> _remainingWallsByTurnOrder;
    private readonly byte _boardSize;
    private readonly byte _ownTurnOrder;
    private readonly List<Wall> _walls;
    private List<BoardCoordinates> _reachableSquares;
    private ITimer? _moveNotificationTimer;

    public MatchPlayViewModel(JoinedMatch joinedMatch, IMatchClient matches, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(joinedMatch);
        ArgumentNullException.ThrowIfNull(matches);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _joinedMatch = joinedMatch;
        _matches = matches;
        _timeProvider = timeProvider;
        _uiContext = SynchronizationContext.Current;
        IReadOnlyList<FoundMatchParticipant> participants = joinedMatch.FoundMatch.Participants;
        _pawnsByTurnOrder = new Dictionary<byte, BoardCoordinates>(participants.Count);
        _turnOrdersByNickname = new Dictionary<string, byte>(participants.Count, StringComparer.Ordinal);
        _nicknamesByTurnOrder = new Dictionary<byte, string>(participants.Count);
        _remainingWallsByTurnOrder = new Dictionary<byte, byte>(participants.Count);
        _walls = new List<Wall>();
        _reachableSquares = new List<BoardCoordinates>();
        _boardSize = joinedMatch.GetBoardSize();
        _ownTurnOrder = joinedMatch.GetOwnTurnOrder();
        CurrentTurnOrder = Match.FirstTurnOrder;
        NextMoveNumber = Match.FirstMoveNumber;
        NoticeText = string.Empty;
        WinnerNickname = string.Empty;
        PlacePawnsAtStart(participants, joinedMatch.FoundMatch.Format.WallsPerPlayer);
        _matches.PawnMoved += HandlePawnMoved;
        _matches.WallPlaced += HandleWallPlaced;
        _matches.MatchAbandoned += HandleMatchAbandoned;
    }

    public byte CurrentTurnOrder { get; private set; }

    public short NextMoveNumber { get; private set; }

    public bool IsMatchOver { get; private set; }

    public string WinnerNickname { get; private set; }

    public bool HasWinner => WinnerNickname.Length > 0;

    public bool IsSending { get; private set; }

    public BoardCoordinates? SelectedPawn { get; private set; }

    public IReadOnlyList<BoardCoordinates> ReachableSquares => _reachableSquares;

    public IReadOnlyDictionary<byte, BoardCoordinates> Pawns => _pawnsByTurnOrder;

    public IReadOnlyList<Wall> Walls => _walls;

    public string NoticeText
    {
        get => Get<string>();
        private set => Set(value);
    }

    [DependsOn(nameof(NoticeText))]
    public bool HasNotice => NoticeText.Length > 0;

    public event EventHandler? StateChanged;

    public async Task SelectSquare(BoardCoordinates square)
    {
        ArgumentNullException.ThrowIfNull(square);

        if (!CanChooseMove())
        {
            return;
        }

        NoticeText = string.Empty;
        if (SelectedPawn is null)
        {
            SelectOwnPawn(square);
            return;
        }

        if (square == SelectedPawn)
        {
            ClearSelection();
            return;
        }

        if (!_reachableSquares.Contains(square))
        {
            NoticeText = Strings.MatchMoveNotAllowed;
            return;
        }

        await SendMove(square);
    }

    public void ClearSelection()
    {
        if (SelectedPawn is null)
        {
            return;
        }

        SelectedPawn = null;
        _reachableSquares = new List<BoardCoordinates>();
        OnStateChanged();
    }

    public Board BuildBoard()
    {
        return new Board(_boardSize, _pawnsByTurnOrder, _walls);
    }

    public byte GetRemainingWalls(byte turnOrder)
    {
        return _remainingWallsByTurnOrder.GetValueOrDefault(turnOrder);
    }

    public string DescribeWallRejection(WallRejectionReason reason, byte? blockedTurnOrder)
    {
        string blockedNickname = blockedTurnOrder is byte turnOrder ? FindNickname(turnOrder) : string.Empty;
        return WallRejectionText.Describe(reason, blockedNickname);
    }

    public bool IsWallTurn()
    {
        byte ownRemainingWalls = GetRemainingWalls(_ownTurnOrder);
        return !IsMatchOver && IsOwnTurn() && ownRemainingWalls > 0;
    }

    public bool CanPlaceWall()
    {
        return !IsSending && IsWallTurn();
    }

    public async Task PlaceWall(Wall wall)
    {
        ArgumentNullException.ThrowIfNull(wall);

        if (!CanPlaceWall())
        {
            return;
        }

        NoticeText = string.Empty;
        string groove = BoardLayout.ToSquare(wall.Groove);
        var orientation = MatchWallTranslator.ToPlacedWallOrientation(wall.Orientation);
        var chosenWall = new ChosenWall
        {
            MatchId = _joinedMatch.FoundMatch.MatchId,
            MoveNumber = NextMoveNumber,
            Groove = groove,
            Orientation = orientation,
        };
        Action placeWall = () => PlaceInJoinedMatch(chosenWall);
        bool isSent = await TrySubmitTurn(placeWall);
        if (isSent)
        {
            WaitForMoveNotification(chosenWall.MoveNumber);
        }
    }

    public void Dispose()
    {
        StopWaitingForMoveNotification();
        _matches.PawnMoved -= HandlePawnMoved;
        _matches.WallPlaced -= HandleWallPlaced;
        _matches.MatchAbandoned -= HandleMatchAbandoned;
    }

    private void PlacePawnsAtStart(IReadOnlyList<FoundMatchParticipant> participants, byte wallsPerPlayer)
    {
        foreach (FoundMatchParticipant participant in participants)
        {
            BoardCoordinates start = BoardLayout.GetStartingCoordinates(_boardSize, participant.TurnOrder);
            _pawnsByTurnOrder[participant.TurnOrder] = start;
            _turnOrdersByNickname[participant.Nickname] = participant.TurnOrder;
            _nicknamesByTurnOrder[participant.TurnOrder] = participant.Nickname;
            _remainingWallsByTurnOrder[participant.TurnOrder] = wallsPerPlayer;
        }
    }

    private void HandlePawnMoved(object? sender, PawnMovedEventArgs e)
    {
        if (!_joinedMatch.IsJoinedMatch(e.MatchId))
        {
            return;
        }

        Action receivePawnMove = () => ReceivePawnMove(e);
        UiThread.Run(_uiContext, receivePawnMove);
    }

    private void ReceivePawnMove(PawnMovedEventArgs pawnMove)
    {
        BoardCoordinates destination = BoardLayout.FindSquare(pawnMove.ToSquare);
        bool isKnownMover = _turnOrdersByNickname.TryGetValue(pawnMove.MoverNickname, out byte moverTurnOrder);
        if (!isKnownMover || destination == BoardLayout.OffBoardSquare)
        {
            _logger.Warn(
                $"A pawn move arrived that the board cannot place. MatchId={pawnMove.MatchId}; "
                + $"Mover={pawnMove.MoverNickname}; Square={pawnMove.ToSquare}");
            return;
        }

        _pawnsByTurnOrder[moverTurnOrder] = destination;
        TakeServerTurn(pawnMove.NextTurnOrder, pawnMove.NextMoveNumber);
        ClearSelection();
        if (pawnMove.IsMatchOver)
        {
            EndMatch(pawnMove.MoverNickname);
            return;
        }

        OnStateChanged();
    }

    private void TakeServerTurn(byte turnOrder, short nextMoveNumber)
    {
        if (nextMoveNumber < NextMoveNumber)
        {
            return;
        }

        CurrentTurnOrder = turnOrder;
        NextMoveNumber = nextMoveNumber;
        StopWaitingForMoveNotification();
    }

    private void StopWaitingForMoveNotification()
    {
        ITimer? timer = Interlocked.Exchange(ref _moveNotificationTimer, value: null);
        if (timer is null)
        {
            return;
        }

        timer.Dispose();
        IsSending = false;
    }

    private void EndMatch(string winnerNickname)
    {
        IsMatchOver = true;
        WinnerNickname = winnerNickname;
        ClearSelection();
        OnStateChanged();
    }

    private void OnStateChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void HandleWallPlaced(object? sender, WallPlacedEventArgs e)
    {
        if (!_joinedMatch.IsJoinedMatch(e.MatchId))
        {
            return;
        }

        Action receiveWallPlacement = () => ReceiveWallPlacement(e);
        UiThread.Run(_uiContext, receiveWallPlacement);
    }

    private void ReceiveWallPlacement(WallPlacedEventArgs wallPlacement)
    {
        BoardCoordinates groove = BoardLayout.FindSquare(wallPlacement.Groove);
        bool isKnownPlacer = _turnOrdersByNickname.TryGetValue(wallPlacement.PlacerNickname, out byte placerTurnOrder);
        if (!isKnownPlacer || groove == BoardLayout.OffBoardSquare)
        {
            _logger.Warn(
                $"A wall arrived that the board cannot place. MatchId={wallPlacement.MatchId}; "
                + $"Placer={wallPlacement.PlacerNickname}; Groove={wallPlacement.Groove}");
            return;
        }

        var orientation = MatchWallTranslator.ToWallOrientation(wallPlacement.Orientation);
        var wall = new Wall(groove, orientation);
        _walls.Add(wall);
        _remainingWallsByTurnOrder[placerTurnOrder] = wallPlacement.RemainingWalls;
        SyncServerTurn(wallPlacement.NextTurnOrder, wallPlacement.NextMoveNumber);
    }

    private void SyncServerTurn(byte turnOrder, short nextMoveNumber)
    {
        TakeServerTurn(turnOrder, nextMoveNumber);
        ClearSelection();
        OnStateChanged();
    }

    private void HandleMatchAbandoned(object? sender, MatchAbandonedEventArgs e)
    {
        if (!_joinedMatch.IsJoinedMatch(e.MatchId))
        {
            return;
        }

        Action receiveAbandonment = () => ReceiveAbandonment(e.AbandoningNickname, e.IsMatchOver);
        UiThread.Run(_uiContext, receiveAbandonment);
    }

    private void ReceiveAbandonment(string abandoningNickname, bool isMatchOver)
    {
        if (isMatchOver)
        {
            EndMatch(_joinedMatch.Session.Nickname);
            return;
        }

        bool isKnownAbandoningPlayer = _turnOrdersByNickname.TryGetValue(abandoningNickname, out byte leftTurnOrder);
        if (!isKnownAbandoningPlayer)
        {
            return;
        }

        _pawnsByTurnOrder.Remove(leftTurnOrder);
        OnStateChanged();
    }

    private bool CanChooseMove()
    {
        return !IsMatchOver && !IsSending;
    }

    private void SelectOwnPawn(BoardCoordinates square)
    {
        BoardCoordinates ownPawn = _pawnsByTurnOrder[_ownTurnOrder];
        if (!IsOwnTurn() || square != ownPawn)
        {
            return;
        }

        Board board = BuildBoard();
        IReadOnlyList<BoardCoordinates> reachableSquares = PawnMovePolicy.ListReachableSquares(board, _ownTurnOrder);
        _reachableSquares = new List<BoardCoordinates>(reachableSquares);
        SelectedPawn = ownPawn;
        OnStateChanged();
    }

    private bool IsOwnTurn()
    {
        return CurrentTurnOrder == _ownTurnOrder;
    }

    private async Task SendMove(BoardCoordinates destination)
    {
        string toSquare = BoardLayout.ToSquare(destination);
        var chosenMove = new ChosenPawnMove(_joinedMatch.FoundMatch.MatchId, NextMoveNumber, toSquare);
        Action moveInJoinedMatch = () => MoveInJoinedMatch(chosenMove);
        bool isSent = await TrySubmitTurn(moveInJoinedMatch);
        if (isSent)
        {
            ClearSelection();
            WaitForMoveNotification(chosenMove.MoveNumber);
        }
    }

    private void MoveInJoinedMatch(ChosenPawnMove chosenMove)
    {
        _matches.MovePawn(_joinedMatch.Session.Token, chosenMove);
    }

    private async Task<bool> TrySubmitTurn(Action turnSubmission)
    {
        IsSending = true;
        try
        {
            await Task.Run(turnSubmission);
            return true;
        }
        catch (NotYourTurnException ex)
        {
            _logger.Warn($"The server says it is not this player's move. NextMoveNumber={ex.NextMoveNumber}", ex);
            SyncServerTurn(ex.CurrentTurnOrder, ex.NextMoveNumber);
        }
        catch (IllegalMoveException ex)
        {
            _logger.Warn("The server turned down a pawn move.", ex);
            NoticeText = Strings.MatchMoveNotAllowed;
        }
        catch (IllegalWallException ex)
        {
            _logger.Warn($"The server turned down a wall. Rejection={ex.Rejection}", ex);
            ShowWallRejection(ex.Rejection, ex.BlockedTurnOrder);
        }
        catch (MatchNotJoinedException ex)
        {
            _logger.Warn("The server says the player is not in that match.", ex);
            EndMatch(winnerNickname: string.Empty);
        }
        catch (InvalidSessionException ex)
        {
            _logger.Warn("The move could not be sent because the session is no longer valid.", ex);
            NoticeText = Strings.MatchMoveFailed;
        }
        catch (ServerUnreachableException ex)
        {
            ReportMoveFailure(ex);
        }
        catch (ConnectionLostException ex)
        {
            ReportMoveFailure(ex);
        }
        catch (ServerErrorException ex)
        {
            ReportMoveFailure(ex);
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The move could not be sent because the connection is closed.", ex);
        }
        finally
        {
            IsSending = false;
        }

        return false;
    }

    private void ShowWallRejection(WallPlacementRejection rejection, byte? blockedTurnOrder)
    {
        WallRejectionReason reason = MatchWallTranslator.ToRejectionReason(rejection);
        NoticeText = DescribeWallRejection(reason, blockedTurnOrder);
    }

    private void ReportMoveFailure(Exception cause)
    {
        _logger.Error("The move could not be sent.", cause);
        NoticeText = Strings.MatchMoveFailed;
    }

    private void WaitForMoveNotification(short sentMoveNumber)
    {
        if (NextMoveNumber > sentMoveNumber)
        {
            return;
        }

        IsSending = true;
        _moveNotificationTimer = _timeProvider.CreateTimer(
            HandleMoveNotificationTimeout,
            state: null,
            _moveNotificationWait,
            Timeout.InfiniteTimeSpan);
    }

    private void HandleMoveNotificationTimeout(object? state)
    {
        UiThread.Run(_uiContext, StopWaitingAfterTimeout);
    }

    private void StopWaitingAfterTimeout()
    {
        if (_moveNotificationTimer is null)
        {
            return;
        }

        _logger.Warn($"The own move did not come back from the server in time. NextMoveNumber={NextMoveNumber}");
        StopWaitingForMoveNotification();
    }

    private string FindNickname(byte turnOrder)
    {
        return _nicknamesByTurnOrder.GetValueOrDefault(turnOrder, string.Empty);
    }

    private void PlaceInJoinedMatch(ChosenWall wall)
    {
        _matches.PlaceWall(_joinedMatch.Session.Token, wall);
    }
}
