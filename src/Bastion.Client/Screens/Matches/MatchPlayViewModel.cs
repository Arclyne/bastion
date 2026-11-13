using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Matches;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Networking.Sessions;
using Bastion.Domain.Matches;
using Gum.Mvvm;
using log4net;

namespace Bastion.Client.Screens.Matches;

public sealed class MatchPlayViewModel : ViewModel, IDisposable
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(MatchPlayViewModel));

    private readonly JoinedMatch _joinedMatch;
    private readonly IMatchClient _matches;
    private readonly SynchronizationContext? _uiContext;
    private readonly Dictionary<byte, BoardCoordinates> _pawnsByTurnOrder;
    private readonly Dictionary<string, byte> _turnOrdersByNickname;
    private readonly Dictionary<byte, string> _nicknamesByTurnOrder;
    private readonly Dictionary<byte, byte> _remainingWallsByTurnOrder;
    private readonly List<Wall> _walls = new List<Wall>();
    private List<BoardCoordinates> _reachableSquares = new List<BoardCoordinates>();

    public MatchPlayViewModel(JoinedMatch joinedMatch, IMatchClient matches)
    {
        ArgumentNullException.ThrowIfNull(joinedMatch);
        ArgumentNullException.ThrowIfNull(matches);

        _joinedMatch = joinedMatch;
        _matches = matches;
        _uiContext = SynchronizationContext.Current;
        IReadOnlyList<FoundMatchParticipant> participants = joinedMatch.FoundMatch.Participants;
        _pawnsByTurnOrder = new Dictionary<byte, BoardCoordinates>(participants.Count);
        _turnOrdersByNickname = new Dictionary<string, byte>(participants.Count, StringComparer.Ordinal);
        _nicknamesByTurnOrder = new Dictionary<byte, string>(participants.Count);
        _remainingWallsByTurnOrder = new Dictionary<byte, byte>(participants.Count);
        BoardSize = joinedMatch.FoundMatch.Format.BoardSize;
        OwnTurnOrder = joinedMatch.FindOwnParticipant().TurnOrder;
        CurrentTurnOrder = Match.FirstTurnOrder;
        NextMoveNumber = Match.FirstMoveNumber;
        NoticeText = string.Empty;
        PlacePawnsAtStart(participants, joinedMatch.FoundMatch.Format.WallsPerPlayer);
        _matches.PawnMoved += HandlePawnMoved;
        _matches.WallPlaced += HandleWallPlaced;
        _matches.MatchAbandoned += HandleMatchAbandoned;
    }

    public byte BoardSize { get; }

    public byte OwnTurnOrder { get; }

    public byte CurrentTurnOrder { get; private set; }

    public short NextMoveNumber { get; private set; }

    public bool IsMatchOver { get; private set; }

    public string? WinnerNickname { get; private set; }

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
        return new Board(BoardSize, _pawnsByTurnOrder, _walls);
    }

    public byte GetRemainingWalls(byte turnOrder)
    {
        return _remainingWallsByTurnOrder.GetValueOrDefault(turnOrder);
    }

    public string FindNickname(byte turnOrder)
    {
        return _nicknamesByTurnOrder.GetValueOrDefault(turnOrder, string.Empty);
    }

    public bool IsOwnTurn()
    {
        return CurrentTurnOrder == OwnTurnOrder;
    }

    public bool CanPlaceWall()
    {
        byte ownRemainingWalls = GetRemainingWalls(OwnTurnOrder);
        return CanChooseMove() && IsOwnTurn() && ownRemainingWalls > Participation.NoWallsLeft;
    }

    public async Task<bool> PlaceWall(Wall wall)
    {
        ArgumentNullException.ThrowIfNull(wall);

        if (!CanPlaceWall())
        {
            return false;
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
        bool isSent = await TrySendToServer(() => PlaceInJoinedMatch(chosenWall));
        return isSent;
    }

    public void Dispose()
    {
        _matches.PawnMoved -= HandlePawnMoved;
        _matches.WallPlaced -= HandleWallPlaced;
        _matches.MatchAbandoned -= HandleMatchAbandoned;
    }

    private void PlacePawnsAtStart(IReadOnlyList<FoundMatchParticipant> participants, byte wallsPerPlayer)
    {
        foreach (FoundMatchParticipant participant in participants)
        {
            BoardCoordinates start = BoardLayout.GetStartingCoordinates(BoardSize, participant.TurnOrder);
            _pawnsByTurnOrder[participant.TurnOrder] = start;
            _turnOrdersByNickname[participant.Nickname] = participant.TurnOrder;
            _nicknamesByTurnOrder[participant.TurnOrder] = participant.Nickname;
            _remainingWallsByTurnOrder[participant.TurnOrder] = wallsPerPlayer;
        }
    }

    private bool CanChooseMove()
    {
        return !IsMatchOver && !IsSending;
    }

    private void SelectOwnPawn(BoardCoordinates square)
    {
        BoardCoordinates ownPawn = _pawnsByTurnOrder[OwnTurnOrder];
        if (!IsOwnTurn() || square != ownPawn)
        {
            return;
        }

        Board board = BuildBoard();
        IReadOnlyList<BoardCoordinates> reachableSquares = PawnMovePolicy.ListReachableSquares(board, OwnTurnOrder);
        _reachableSquares = new List<BoardCoordinates>(reachableSquares);
        SelectedPawn = ownPawn;
        OnStateChanged();
    }

    private async Task SendMove(BoardCoordinates destination)
    {
        string toSquare = BoardLayout.ToSquare(destination);
        var move = new ChosenPawnMove(_joinedMatch.FoundMatch.MatchId, NextMoveNumber, toSquare);
        bool isSent = await TrySendToServer(() => MoveInJoinedMatch(move));
        if (isSent)
        {
            ClearSelection();
        }
    }

    private async Task<bool> TrySendToServer(Action sendToServer)
    {
        IsSending = true;
        try
        {
            await Task.Run(sendToServer);
            return true;
        }
        catch (NotYourTurnException ex)
        {
            _logger.Warn($"The server says another player has the turn. NextMoveNumber={ex.NextMoveNumber}", ex);
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
            ShowWallRejection(ex);
        }
        catch (MatchNotJoinedException ex)
        {
            _logger.Warn("The match was already over when the move was sent.", ex);
            EndMatch(null);
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

    private void MoveInJoinedMatch(ChosenPawnMove move)
    {
        _matches.MovePawn(_joinedMatch.Session.Token, move);
    }

    private void PlaceInJoinedMatch(ChosenWall wall)
    {
        _matches.PlaceWall(_joinedMatch.Session.Token, wall);
    }

    private void SyncServerTurn(byte turnOrder, short nextMoveNumber)
    {
        TakeServerTurn(turnOrder, nextMoveNumber);
        ClearSelection();
        OnStateChanged();
    }

    private void ShowWallRejection(IllegalWallException rejection)
    {
        WallRejectionReason reason = MatchWallTranslator.ToRejectionReason(rejection.Rejection);
        string blockedNickname = rejection.BlockedTurnOrder is byte blockedTurnOrder
            ? FindNickname(blockedTurnOrder)
            : string.Empty;
        NoticeText = WallRejectionText.Describe(reason, blockedNickname);
    }

    private void ReportMoveFailure(Exception ex)
    {
        _logger.Error("The move could not be sent.", ex);
        NoticeText = Strings.MatchMoveFailed;
    }

    private void ReceivePawnMove(PawnMovedEventArgs pawnMove)
    {
        if (!_turnOrdersByNickname.TryGetValue(pawnMove.MoverNickname, out byte moverTurnOrder)
            || !BoardLayout.TryParseSquare(pawnMove.ToSquare, out BoardCoordinates destination))
        {
            _logger.Warn(
                $"A pawn move arrived that the board cannot place. MatchId={pawnMove.MatchId}; "
                + $"Square={pawnMove.ToSquare}");
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

    private void ReceiveWallPlacement(WallPlacedEventArgs wallPlacement)
    {
        if (!_turnOrdersByNickname.TryGetValue(wallPlacement.PlacerNickname, out byte placerTurnOrder)
            || !BoardLayout.TryParseSquare(wallPlacement.Groove, out BoardCoordinates groove))
        {
            _logger.Warn(
                $"A wall arrived that the board cannot place. MatchId={wallPlacement.MatchId}; "
                + $"Groove={wallPlacement.Groove}");
            return;
        }

        var orientation = MatchWallTranslator.ToWallOrientation(wallPlacement.Orientation);
        var wall = new Wall(groove, orientation);
        _walls.Add(wall);
        _remainingWallsByTurnOrder[placerTurnOrder] = wallPlacement.RemainingWalls;
        TakeServerTurn(wallPlacement.NextTurnOrder, wallPlacement.NextMoveNumber);
        ClearSelection();
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
    }

    private void ReceiveAbandonment(string abandoningNickname, bool isMatchOver)
    {
        if (isMatchOver)
        {
            EndMatch(_joinedMatch.Session.Nickname);
            return;
        }

        if (!_turnOrdersByNickname.TryGetValue(abandoningNickname, out byte leftTurnOrder))
        {
            return;
        }

        _pawnsByTurnOrder.Remove(leftTurnOrder);
        OnStateChanged();
    }

    private void EndMatch(string? winnerNickname)
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

    private void HandlePawnMoved(object? sender, PawnMovedEventArgs e)
    {
        if (e.MatchId != _joinedMatch.FoundMatch.MatchId)
        {
            return;
        }

        UiThread.Run(_uiContext, () => ReceivePawnMove(e));
    }

    private void HandleWallPlaced(object? sender, WallPlacedEventArgs e)
    {
        if (e.MatchId != _joinedMatch.FoundMatch.MatchId)
        {
            return;
        }

        UiThread.Run(_uiContext, () => ReceiveWallPlacement(e));
    }

    private void HandleMatchAbandoned(object? sender, MatchAbandonedEventArgs e)
    {
        if (e.MatchId != _joinedMatch.FoundMatch.MatchId)
        {
            return;
        }

        UiThread.Run(_uiContext, () => ReceiveAbandonment(e.AbandoningNickname, e.IsMatchOver));
    }
}
