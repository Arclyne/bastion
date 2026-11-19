using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Matches;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Screens.Matches;
using Bastion.Client.Tests.Fakes;
using Bastion.Domain.Matches;
using Xunit;

namespace Bastion.Client.Tests.Screens.Matches;

public sealed class TestMatchPlayViewModel : IDisposable
{
    private const int OtherMatchId = 40;
    private const string ForwardSquare = "e2";
    private const string FirstGoalSquare = "e9";
    private const int CenterColumnIndex = 4;
    private const int RightColumnIndex = 5;
    private const int LeftColumnIndex = 3;
    private const int FirstRowIndex = 0;
    private const int SecondRowIndex = 1;
    private const int ThirdRowIndex = 2;
    private const int LastRowIndex = 8;
    private const int BeforeLastRowIndex = 7;
    private const int FourthRowIndex = 3;
    private const string GrooveAheadOfSecondPawn = "e8";
    private const string CenterGroove = "e4";
    private const short ServerNextMoveNumber = 2;

    private static readonly BoardCoordinates _firstStart = new BoardCoordinates(CenterColumnIndex, FirstRowIndex);
    private static readonly BoardCoordinates _secondStart = new BoardCoordinates(CenterColumnIndex, LastRowIndex);
    private static readonly BoardCoordinates _forward = new BoardCoordinates(CenterColumnIndex, SecondRowIndex);
    private static readonly BoardCoordinates _rightOfFirstStart = new BoardCoordinates(RightColumnIndex, FirstRowIndex);
    private static readonly BoardCoordinates _leftOfFirstStart = new BoardCoordinates(LeftColumnIndex, FirstRowIndex);
    private static readonly BoardCoordinates _secondForward = new BoardCoordinates(
        CenterColumnIndex,
        BeforeLastRowIndex);
    private static readonly BoardCoordinates _centerGrooveCoordinates = new BoardCoordinates(
        CenterColumnIndex,
        FourthRowIndex);
    private static readonly BoardCoordinates _twoSquaresForward = new BoardCoordinates(
        CenterColumnIndex,
        ThirdRowIndex);
    private static readonly Wall _centerWall = new Wall(_centerGrooveCoordinates, WallOrientation.Horizontal);
    private static readonly DateTimeOffset _startedAt = new DateTimeOffset(2026, 11, 15, 18, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan _moveNotificationWait = TimeSpan.FromSeconds(
        MatchPlayViewModel.MoveNotificationWaitSeconds);

    private readonly FakeMatchClient _matchClient = new FakeMatchClient();
    private readonly ManualTimeProvider _timeProvider = new ManualTimeProvider(_startedAt);
    private readonly List<MatchPlayViewModel> _viewModels = [];

    public void Dispose()
    {
        _viewModels.ForEach(DisposeViewModel);
    }

    [Fact]
    public async Task SelectSquare_OwnPawnOnOwnTurn_ListsReachableSquares()
    {
        MatchPlayViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);
        BoardCoordinates[] expectedSquares = { _forward, _rightOfFirstStart, _leftOfFirstStart };

        await viewModel.SelectSquare(_firstStart);

        Assert.Equal(expectedSquares, viewModel.ReachableSquares);
    }

    [Fact]
    public async Task SelectSquare_OwnPawnOnRivalTurn_ListsNoSquares()
    {
        MatchPlayViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);

        await viewModel.SelectSquare(_secondStart);

        Assert.Empty(viewModel.ReachableSquares);
    }

    [Fact]
    public async Task SelectSquare_SelectedPawnAgain_ClearsReachableSquares()
    {
        MatchPlayViewModel viewModel = await CreateViewModelWithSelectedPawn();

        await viewModel.SelectSquare(_firstStart);

        Assert.Empty(viewModel.ReachableSquares);
    }

    [Fact]
    public async Task SelectSquare_ReachableSquare_SendsMoveWithNextMoveNumber()
    {
        var expectedMove = new ChosenPawnMove(JoinedMatchSampleFactory.MatchId, Match.FirstMoveNumber, ForwardSquare);
        MatchPlayViewModel viewModel = await CreateViewModelWithSelectedPawn();

        await viewModel.SelectSquare(_forward);

        Assert.Equal(expectedMove, _matchClient.LastMove);
    }

    [Fact]
    public async Task SelectSquare_UnreachableSquare_SendsNothing()
    {
        MatchPlayViewModel viewModel = await CreateViewModelWithSelectedPawn();

        await viewModel.SelectSquare(_twoSquaresForward);

        Assert.Null(_matchClient.LastMove);
    }

    [Fact]
    public async Task SelectSquare_UnreachableSquare_ShowsMoveNotAllowed()
    {
        MatchPlayViewModel viewModel = await CreateViewModelWithSelectedPawn();

        await viewModel.SelectSquare(_twoSquaresForward);

        Assert.Equal(Strings.MatchMoveNotAllowed, viewModel.NoticeText);
    }

    [Fact]
    public async Task SelectSquare_NotYourTurn_TakesServerMoveNumber()
    {
        MatchPlayViewModel viewModel = await CreateViewModelWithSelectedPawn();
        var cause = new InvalidOperationException();
        var notYourTurn = new NotYourTurnException(Match.SecondTurnOrder, ServerNextMoveNumber, cause);
        _matchClient.MoveFailure = notYourTurn;

        await viewModel.SelectSquare(_forward);

        Assert.Equal(ServerNextMoveNumber, viewModel.NextMoveNumber);
    }

    [Fact]
    public async Task SelectSquare_IllegalMove_ShowsMoveNotAllowed()
    {
        MatchPlayViewModel viewModel = await CreateViewModelWithSelectedPawn();
        _matchClient.MoveFailure = new IllegalMoveException();

        await viewModel.SelectSquare(_forward);

        Assert.Equal(Strings.MatchMoveNotAllowed, viewModel.NoticeText);
    }

    [Fact]
    public async Task SelectSquare_ServerUnreachable_ShowsMoveFailed()
    {
        MatchPlayViewModel viewModel = await CreateViewModelWithSelectedPawn();
        _matchClient.MoveFailure = new ServerUnreachableException();

        await viewModel.SelectSquare(_forward);

        Assert.Equal(Strings.MatchMoveFailed, viewModel.NoticeText);
    }

    [Fact]
    public async Task SelectSquare_MatchAlreadyOver_EndsMatch()
    {
        MatchPlayViewModel viewModel = await CreateViewModelWithSelectedPawn();
        _matchClient.MoveFailure = new MatchNotJoinedException();

        await viewModel.SelectSquare(_forward);

        Assert.True(viewModel.IsMatchOver);
    }

    [Fact]
    public async Task SelectSquare_MatchAlreadyOver_NamesNoWinner()
    {
        MatchPlayViewModel viewModel = await CreateViewModelWithSelectedPawn();
        _matchClient.MoveFailure = new MatchNotJoinedException();

        await viewModel.SelectSquare(_forward);

        Assert.False(viewModel.HasWinner);
    }

    [Fact]
    public void PawnMoved_RivalMoved_MovesRivalPawn()
    {
        MatchPlayViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);
        PawnMovedEventArgs rivalMove = CreateRivalFirstMove(JoinedMatchSampleFactory.MatchId);

        _matchClient.RaisePawnMoved(rivalMove);

        BoardCoordinates rivalPawn = viewModel.Pawns[Match.FirstTurnOrder];
        Assert.Equal(_forward, rivalPawn);
    }

    [Fact]
    public void PawnMoved_RivalMoved_GivesTurnToOwnPawn()
    {
        MatchPlayViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);
        PawnMovedEventArgs rivalMove = CreateRivalFirstMove(JoinedMatchSampleFactory.MatchId);

        _matchClient.RaisePawnMoved(rivalMove);

        Assert.Equal(Match.SecondTurnOrder, viewModel.CurrentTurnOrder);
    }

    [Fact]
    public void PawnMoved_OtherMatch_KeepsRivalPawn()
    {
        MatchPlayViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);
        PawnMovedEventArgs otherMatchMove = CreateRivalFirstMove(OtherMatchId);

        _matchClient.RaisePawnMoved(otherMatchMove);

        BoardCoordinates rivalPawn = viewModel.Pawns[Match.FirstTurnOrder];
        Assert.Equal(_firstStart, rivalPawn);
    }

    [Fact]
    public void PawnMoved_RivalReachedGoal_NamesRivalWinner()
    {
        MatchPlayViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);
        PawnMovedEventArgs winningMove = PawnMovedSampleFactory.CreateWinningMove(
            JoinedMatchSampleFactory.RivalNickname,
            FirstGoalSquare);

        _matchClient.RaisePawnMoved(winningMove);

        Assert.Equal(JoinedMatchSampleFactory.RivalNickname, viewModel.WinnerNickname);
    }

    [Fact]
    public void MatchAbandoned_MatchOver_EndsMatch()
    {
        MatchPlayViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);

        _matchClient.RaiseMatchAbandoned(
            JoinedMatchSampleFactory.MatchId,
            JoinedMatchSampleFactory.RivalNickname,
            isMatchOver: true);

        Assert.True(viewModel.IsMatchOver);
    }

    [Fact]
    public void WallPlaced_RivalPlacedWall_AddsWall()
    {
        MatchPlayViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);
        WallPlacedEventArgs rivalWall = CreateRivalFirstWall(CenterGroove);

        _matchClient.RaiseWallPlaced(rivalWall);

        Wall placedWall = viewModel.Walls.Single();
        Assert.Equal(_centerWall, placedWall);
    }

    [Fact]
    public void WallPlaced_RivalPlacedWall_UpdatesRivalWallsLeft()
    {
        MatchPlayViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);
        WallPlacedEventArgs rivalWall = CreateRivalFirstWall(CenterGroove);

        _matchClient.RaiseWallPlaced(rivalWall);

        byte rivalWallsLeft = viewModel.GetRemainingWalls(Match.FirstTurnOrder);
        Assert.Equal(WallPlacedSampleFactory.WallsLeftAfterFirstWall, rivalWallsLeft);
    }

    [Fact]
    public void WallPlaced_RivalPlacedWall_GivesTurnToOwnPawn()
    {
        MatchPlayViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);
        WallPlacedEventArgs rivalWall = CreateRivalFirstWall(CenterGroove);

        _matchClient.RaiseWallPlaced(rivalWall);

        Assert.Equal(Match.SecondTurnOrder, viewModel.CurrentTurnOrder);
    }

    [Fact]
    public async Task SelectSquare_WallAheadOfOwnPawn_ExcludesSquareBehindWall()
    {
        MatchPlayViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);
        WallPlacedEventArgs rivalWall = CreateRivalFirstWall(GrooveAheadOfSecondPawn);
        _matchClient.RaiseWallPlaced(rivalWall);

        await viewModel.SelectSquare(_secondStart);

        Assert.DoesNotContain(_secondForward, viewModel.ReachableSquares);
    }

    [Fact]
    public async Task PlaceWall_OwnTurn_SendsWallWithNextMoveNumber()
    {
        var expectedWall = new ChosenWall
        {
            MatchId = JoinedMatchSampleFactory.MatchId,
            MoveNumber = Match.FirstMoveNumber,
            Groove = CenterGroove,
            Orientation = PlacedWallOrientation.Horizontal,
        };
        MatchPlayViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);

        await viewModel.PlaceWall(_centerWall);

        Assert.Equal(expectedWall, _matchClient.LastWall);
    }

    [Fact]
    public async Task PlaceWall_RivalTurn_SendsNothing()
    {
        MatchPlayViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);

        await viewModel.PlaceWall(_centerWall);

        Assert.Null(_matchClient.LastWall);
    }

    [Fact]
    public async Task PlaceWall_GrooveTaken_ShowsGrooveTaken()
    {
        MatchPlayViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);
        _matchClient.WallFailure = CreateIllegalWall(WallPlacementRejection.GrooveTaken, blockedTurnOrder: null);

        await viewModel.PlaceWall(_centerWall);

        Assert.Equal(Strings.MatchWallGrooveTaken, viewModel.NoticeText);
    }

    [Fact]
    public async Task PlaceWall_WouldBlockRival_NamesRival()
    {
        string expectedText = string.Format(
            CultureInfo.CurrentCulture,
            Strings.MatchWallWouldBlockFormat,
            JoinedMatchSampleFactory.RivalNickname);
        MatchPlayViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);
        _matchClient.WallFailure = CreateIllegalWall(WallPlacementRejection.WouldBlockPlayer, Match.SecondTurnOrder);

        await viewModel.PlaceWall(_centerWall);

        Assert.Equal(expectedText, viewModel.NoticeText);
    }

    [Fact]
    public async Task PlaceWall_SentBeforeWallPlaced_BlocksNextWall()
    {
        MatchPlayViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);

        await viewModel.PlaceWall(_centerWall);

        bool canPlaceWall = viewModel.CanPlaceWall();
        Assert.False(canPlaceWall);
    }

    [Fact]
    public async Task WallPlaced_OwnWallSent_StopsSending()
    {
        MatchPlayViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);
        WallPlacedEventArgs ownWall = WallPlacedSampleFactory.CreateFirstWall(
            JoinedMatchSampleFactory.OwnNickname,
            CenterGroove);
        await viewModel.PlaceWall(_centerWall);

        _matchClient.RaiseWallPlaced(ownWall);

        Assert.False(viewModel.IsSending);
    }

    [Fact]
    public async Task ClockTick_NoWallPlacedNotification_StopsSending()
    {
        MatchPlayViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);
        await viewModel.PlaceWall(_centerWall);

        _timeProvider.Advance(_moveNotificationWait);

        Assert.False(viewModel.IsSending);
    }

    [Fact]
    public void Dispose_Subscribed_StopsListeningToPawnMoves()
    {
        MatchPlayViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);

        viewModel.Dispose();

        bool hasSubscribers = _matchClient.HasPawnMovedSubscribers();
        Assert.False(hasSubscribers);
    }

    private static void DisposeViewModel(MatchPlayViewModel viewModel)
    {
        viewModel.Dispose();
    }

    private async Task<MatchPlayViewModel> CreateViewModelWithSelectedPawn()
    {
        MatchPlayViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);
        await viewModel.SelectSquare(_firstStart);
        return viewModel;
    }

    private MatchPlayViewModel CreateViewModel(byte ownTurnOrder)
    {
        JoinedMatch joinedMatch = JoinedMatchSampleFactory.CreateWithOwnTurnOrder(ownTurnOrder);
        Func<MatchPlayViewModel> createViewModel = () => new MatchPlayViewModel(
            joinedMatch,
            _matchClient,
            _timeProvider);
        MatchPlayViewModel viewModel = InlineUiContext.CreateViewModel(createViewModel);
        _viewModels.Add(viewModel);
        return viewModel;
    }

    private static PawnMovedEventArgs CreateRivalFirstMove(int matchId)
    {
        return PawnMovedSampleFactory.CreateFirstMove(matchId, JoinedMatchSampleFactory.RivalNickname, ForwardSquare);
    }

    private static WallPlacedEventArgs CreateRivalFirstWall(string groove)
    {
        return WallPlacedSampleFactory.CreateFirstWall(JoinedMatchSampleFactory.RivalNickname, groove);
    }

    private static IllegalWallException CreateIllegalWall(WallPlacementRejection rejection, byte? blockedTurnOrder)
    {
        var cause = new InvalidOperationException();
        return new IllegalWallException(rejection, blockedTurnOrder, cause);
    }
}
