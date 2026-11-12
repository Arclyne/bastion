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

public class TestMatchPlayViewModel : IDisposable
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

    private static readonly BoardCoordinates _firstStart = new BoardCoordinates(CenterColumnIndex, FirstRowIndex);
    private static readonly BoardCoordinates _secondStart = new BoardCoordinates(CenterColumnIndex, LastRowIndex);
    private static readonly BoardCoordinates _forward = new BoardCoordinates(CenterColumnIndex, SecondRowIndex);
    private static readonly BoardCoordinates _secondForward =
        new BoardCoordinates(CenterColumnIndex, BeforeLastRowIndex);
    private static readonly BoardCoordinates _centerGrooveCoordinates =
        new BoardCoordinates(CenterColumnIndex, FourthRowIndex);
    private static readonly BoardCoordinates _twoSquaresForward =
        new BoardCoordinates(CenterColumnIndex, ThirdRowIndex);

    private readonly FakeMatchClient _matchClient = new FakeMatchClient();
    private readonly List<MatchPlayViewModel> _viewModels = new List<MatchPlayViewModel>();

    public void Dispose()
    {
        _viewModels.ForEach(DisposeViewModel);
    }

    [Fact]
    public async Task SelectSquare_OwnPawnOnOwnTurn_ListsReachableSquares()
    {
        var expectedSquares = new BoardCoordinates[]
        {
            _forward,
            new BoardCoordinates(RightColumnIndex, FirstRowIndex),
            new BoardCoordinates(LeftColumnIndex, FirstRowIndex),
        };
        MatchPlayViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);

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
        MatchPlayViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);
        await viewModel.SelectSquare(_firstStart);

        await viewModel.SelectSquare(_firstStart);

        Assert.Empty(viewModel.ReachableSquares);
    }

    [Fact]
    public async Task SelectSquare_ReachableSquare_SendsMoveWithNextMoveNumber()
    {
        var expectedMove = new ChosenPawnMove(JoinedMatchSamples.MatchId, Match.FirstMoveNumber, ForwardSquare);
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
        _matchClient.MoveFailure = new NotYourTurnException(
            Match.SecondTurnOrder,
            PawnMovedSamples.SecondMoveNumber,
            new InvalidOperationException());

        await viewModel.SelectSquare(_forward);

        Assert.Equal(PawnMovedSamples.SecondMoveNumber, viewModel.NextMoveNumber);
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
    public void PawnMoved_RivalMoved_MovesRivalPawn()
    {
        MatchPlayViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);

        _matchClient.RaisePawnMoved(CreateRivalFirstMove(JoinedMatchSamples.MatchId));

        Assert.Equal(_forward, viewModel.Pawns[Match.FirstTurnOrder]);
    }

    [Fact]
    public void PawnMoved_RivalMoved_GivesTurnToOwnPawn()
    {
        MatchPlayViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);

        _matchClient.RaisePawnMoved(CreateRivalFirstMove(JoinedMatchSamples.MatchId));

        Assert.Equal(Match.SecondTurnOrder, viewModel.CurrentTurnOrder);
    }

    [Fact]
    public void PawnMoved_OtherMatch_KeepsRivalPawn()
    {
        MatchPlayViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);

        _matchClient.RaisePawnMoved(CreateRivalFirstMove(OtherMatchId));

        Assert.Equal(_firstStart, viewModel.Pawns[Match.FirstTurnOrder]);
    }

    [Fact]
    public void PawnMoved_RivalReachedGoal_NamesRivalWinner()
    {
        MatchPlayViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);

        _matchClient.RaisePawnMoved(
            PawnMovedSamples.CreateWinningMove(JoinedMatchSamples.RivalNickname, FirstGoalSquare));

        Assert.Equal(JoinedMatchSamples.RivalNickname, viewModel.WinnerNickname);
    }

    [Fact]
    public void MatchAbandoned_MatchOver_EndsMatch()
    {
        MatchPlayViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);

        _matchClient.RaiseMatchAbandoned(JoinedMatchSamples.MatchId, JoinedMatchSamples.RivalNickname, true);

        Assert.True(viewModel.IsMatchOver);
    }

    [Fact]
    public void WallPlaced_RivalPlacedWall_AddsWall()
    {
        var expectedWall = new Wall(_centerGrooveCoordinates, WallOrientation.Horizontal);
        MatchPlayViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);

        _matchClient.RaiseWallPlaced(WallPlacedSamples.CreateFirstWall(JoinedMatchSamples.RivalNickname, CenterGroove));

        Assert.Equal(expectedWall, viewModel.Walls.Single());
    }

    [Fact]
    public void WallPlaced_RivalPlacedWall_UpdatesRivalWallsLeft()
    {
        MatchPlayViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);

        _matchClient.RaiseWallPlaced(WallPlacedSamples.CreateFirstWall(JoinedMatchSamples.RivalNickname, CenterGroove));

        Assert.Equal(WallPlacedSamples.WallsLeftAfterOne, viewModel.GetRemainingWalls(Match.FirstTurnOrder));
    }

    [Fact]
    public void WallPlaced_RivalPlacedWall_GivesTurnToOwnPawn()
    {
        MatchPlayViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);

        _matchClient.RaiseWallPlaced(WallPlacedSamples.CreateFirstWall(JoinedMatchSamples.RivalNickname, CenterGroove));

        Assert.Equal(Match.SecondTurnOrder, viewModel.CurrentTurnOrder);
    }

    [Fact]
    public async Task SelectSquare_WallAheadOfOwnPawn_ExcludesSquareBehindWall()
    {
        MatchPlayViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);
        _matchClient.RaiseWallPlaced(
            WallPlacedSamples.CreateFirstWall(JoinedMatchSamples.RivalNickname, GrooveAheadOfSecondPawn));

        await viewModel.SelectSquare(_secondStart);

        Assert.DoesNotContain(_secondForward, viewModel.ReachableSquares);
    }

    [Fact]
    public async Task PlaceWall_OwnTurn_SendsWallWithNextMoveNumber()
    {
        var expectedWall = new ChosenWall
        {
            MatchId = JoinedMatchSamples.MatchId,
            MoveNumber = Match.FirstMoveNumber,
            Groove = CenterGroove,
            Orientation = PlacedWallOrientation.Horizontal,
        };
        MatchPlayViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);

        await viewModel.PlaceWall(new Wall(_centerGrooveCoordinates, WallOrientation.Horizontal));

        Assert.Equal(expectedWall, _matchClient.LastWall);
    }

    [Fact]
    public async Task PlaceWall_RivalTurn_SendsNothing()
    {
        MatchPlayViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);

        await viewModel.PlaceWall(new Wall(_centerGrooveCoordinates, WallOrientation.Horizontal));

        Assert.Null(_matchClient.LastWall);
    }

    [Fact]
    public async Task PlaceWall_GrooveTaken_ShowsGrooveTaken()
    {
        MatchPlayViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);
        _matchClient.WallFailure = new IllegalWallException(
            WallPlacementRejection.GrooveTaken,
            null,
            new InvalidOperationException());

        await viewModel.PlaceWall(new Wall(_centerGrooveCoordinates, WallOrientation.Horizontal));

        Assert.Equal(Strings.MatchWallGrooveTaken, viewModel.NoticeText);
    }

    [Fact]
    public async Task PlaceWall_WouldBlockRival_NamesRival()
    {
        string expectedText = string.Format(
            CultureInfo.CurrentCulture,
            Strings.MatchWallWouldBlockFormat,
            JoinedMatchSamples.RivalNickname);
        MatchPlayViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);
        _matchClient.WallFailure = new IllegalWallException(
            WallPlacementRejection.WouldBlockPlayer,
            Match.SecondTurnOrder,
            new InvalidOperationException());

        await viewModel.PlaceWall(new Wall(_centerGrooveCoordinates, WallOrientation.Horizontal));

        Assert.Equal(expectedText, viewModel.NoticeText);
    }

    [Fact]
    public void Dispose_Subscribed_StopsListeningToPawnMoves()
    {
        MatchPlayViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);

        viewModel.Dispose();

        Assert.False(_matchClient.HasPawnMovedSubscribers());
    }

    private static void DisposeViewModel(MatchPlayViewModel viewModel)
    {
        viewModel.Dispose();
    }

    private static PawnMovedEventArgs CreateRivalFirstMove(int matchId)
    {
        return PawnMovedSamples.CreateFirstMove(matchId, JoinedMatchSamples.RivalNickname, ForwardSquare);
    }

    private async Task<MatchPlayViewModel> CreateViewModelWithSelectedPawn()
    {
        MatchPlayViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);
        await viewModel.SelectSquare(_firstStart);
        return viewModel;
    }

    private MatchPlayViewModel CreateViewModel(byte ownTurnOrder)
    {
        JoinedMatch joinedMatch = JoinedMatchSamples.CreateWithOwnTurnOrder(ownTurnOrder);
        MatchPlayViewModel viewModel =
            InlineUiContext.CreateViewModel(() => new MatchPlayViewModel(joinedMatch, _matchClient));
        _viewModels.Add(viewModel);
        return viewModel;
    }
}
