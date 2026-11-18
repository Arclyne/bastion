using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Screens.Matches;
using Bastion.Client.Tests.Fakes;
using Bastion.Domain.Matches;
using Xunit;

namespace Bastion.Client.Tests.Screens.Matches;

public sealed class TestMatchViewModel : IDisposable
{
    private const int ClassicCenterColumnIndex = 4;
    private const int FirstRowIndex = 0;
    private const int SecondRowIndex = 1;
    private const string SecondRowSquare = "e2";
    private const string FirstGoalSquare = "e9";
    private const string CenterGroove = "e4";
    private const int ClassicLastRowIndex = 8;
    private const string FiveMinuteClockText = "5:00";
    private const int FourPlayers = 4;
    private const int FirstPlayerIndex = 0;
    private const int SecondPlayerIndex = 1;
    private const int FourthPlayerIndex = 3;

    private static readonly BoardCoordinates _rivalForward = new BoardCoordinates(ClassicCenterColumnIndex, SecondRowIndex);
    private static readonly BoardPawn _movedRivalPawn = new BoardPawn(_rivalForward, PawnSymbol.Blue);
    private static readonly PawnSymbol[] _symbolsWithoutFourthPlayer =
    {
        PawnSymbol.Blue,
        PawnSymbol.Red,
        PawnSymbol.Green,
    };
    private static readonly DateTimeOffset _startedAt = new DateTimeOffset(2026, 11, 15, 18, 0, 0, TimeSpan.Zero);

    private readonly FakeMatchClient _matchClient = new FakeMatchClient();
    private readonly ManualTimeProvider _timeProvider = new ManualTimeProvider(_startedAt);
    private readonly List<IDisposable> _createdViewModels = new List<IDisposable>();

    public void Dispose()
    {
        _createdViewModels.ForEach(DisposeViewModel);
    }

    [Fact]
    public void Constructor_RivalListedFirst_OrdersPlayersByTurnOrder()
    {
        MatchViewModelParts parts = CreateParts(Match.FirstTurnOrder);
        byte[] expectedTurnOrders = { Match.FirstTurnOrder, Match.SecondTurnOrder };

        using var viewModel = new MatchViewModel(parts.JoinedMatch, parts.Abandonment, parts.MatchPlay);

        IEnumerable<byte> turnOrders = viewModel.Players.Select(player => player.TurnOrder);
        Assert.Equal(expectedTurnOrders, turnOrders);
    }

    [Fact]
    public void Constructor_RivalListedFirst_MarksOwnPlayer()
    {
        MatchViewModelParts parts = CreateParts(Match.FirstTurnOrder);

        using var viewModel = new MatchViewModel(parts.JoinedMatch, parts.Abandonment, parts.MatchPlay);

        MatchPlayerViewModel ownPlayer = viewModel.Players.Single(player => player.IsOwn);
        Assert.Equal(JoinedMatchSampleFactory.OwnNickname, ownPlayer.Nickname);
    }

    [Fact]
    public void Constructor_OwnSecondTurnOrder_KeepsOwnTurnOrder()
    {
        MatchViewModelParts parts = CreateParts(Match.SecondTurnOrder);

        using var viewModel = new MatchViewModel(parts.JoinedMatch, parts.Abandonment, parts.MatchPlay);

        Assert.Equal(Match.SecondTurnOrder, viewModel.OwnTurnOrder);
    }

    [Fact]
    public void Constructor_OwnFirstTurnOrder_ShowsOwnTurn()
    {
        MatchViewModelParts parts = CreateParts(Match.FirstTurnOrder);

        using var viewModel = new MatchViewModel(parts.JoinedMatch, parts.Abandonment, parts.MatchPlay);

        Assert.Equal(Strings.MatchYourTurn, viewModel.TurnText);
    }

    [Fact]
    public void Constructor_OwnSecondTurnOrder_ShowsRivalTurn()
    {
        MatchViewModelParts parts = CreateParts(Match.SecondTurnOrder);

        using var viewModel = new MatchViewModel(parts.JoinedMatch, parts.Abandonment, parts.MatchPlay);

        Assert.Equal(Strings.MatchRivalTurn, viewModel.TurnText);
    }

    [Fact]
    public void Constructor_FirstTurnOrderPlayer_HasTurn()
    {
        MatchViewModelParts parts = CreateParts(Match.SecondTurnOrder);

        using var viewModel = new MatchViewModel(parts.JoinedMatch, parts.Abandonment, parts.MatchPlay);

        MatchPlayerViewModel firstPlayer = viewModel.Players[FirstPlayerIndex];
        Assert.True(firstPlayer.HasTurn);
    }

    [Fact]
    public void PawnMoved_RivalMoved_ShowsOwnTurn()
    {
        MatchViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);
        PawnMovedEventArgs rivalMove = CreateRivalFirstMove();

        _matchClient.RaisePawnMoved(rivalMove);

        Assert.Equal(Strings.MatchYourTurn, viewModel.TurnText);
    }

    [Fact]
    public void PawnMoved_RivalMoved_GivesTurnHighlightToOwnRow()
    {
        MatchViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);
        PawnMovedEventArgs rivalMove = CreateRivalFirstMove();

        _matchClient.RaisePawnMoved(rivalMove);

        MatchPlayerViewModel ownPlayer = viewModel.Players[SecondPlayerIndex];
        Assert.True(ownPlayer.HasTurn);
    }

    [Fact]
    public void PawnMoved_RivalMoved_MovesRivalPawn()
    {
        MatchViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);
        PawnMovedEventArgs rivalMove = CreateRivalFirstMove();

        _matchClient.RaisePawnMoved(rivalMove);

        Assert.Contains(_movedRivalPawn, viewModel.Pawns);
    }

    [Fact]
    public void Constructor_RivalMovedDuringVersus_StartsWithMovedPawn()
    {
        MatchViewModelParts parts = CreateParts(Match.SecondTurnOrder);
        PawnMovedEventArgs rivalMove = CreateRivalFirstMove();
        _matchClient.RaisePawnMoved(rivalMove);

        using var viewModel = new MatchViewModel(parts.JoinedMatch, parts.Abandonment, parts.MatchPlay);

        Assert.Contains(_movedRivalPawn, viewModel.Pawns);
    }

    [Fact]
    public void WallPlaced_RivalPlacedWall_ShowsRivalWallsLeft()
    {
        string expectedText = string.Format(
            CultureInfo.CurrentCulture,
            Strings.MatchWallsFormat,
            WallPlacedSampleFactory.WallsLeftAfterFirstWall);
        MatchViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);
        WallPlacedEventArgs rivalWall = WallPlacedSampleFactory.CreateFirstWall(
            JoinedMatchSampleFactory.RivalNickname,
            CenterGroove);

        _matchClient.RaiseWallPlaced(rivalWall);

        MatchPlayerViewModel rivalPlayer = viewModel.Players[FirstPlayerIndex];
        Assert.Equal(expectedText, rivalPlayer.WallsText);
    }

    [Fact]
    public void PawnMoved_OwnPawnReachedGoal_ShowsOwnWin()
    {
        MatchViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);
        PawnMovedEventArgs winningMove = PawnMovedSampleFactory.CreateWinningMove(
            JoinedMatchSampleFactory.OwnNickname,
            FirstGoalSquare);

        _matchClient.RaisePawnMoved(winningMove);

        Assert.Equal(Strings.MatchYouWon, viewModel.TurnText);
    }

    [Fact]
    public void PawnMoved_RivalReachedGoal_ShowsRivalWin()
    {
        string expectedText = string.Format(
            CultureInfo.CurrentCulture,
            Strings.MatchPlayerWonFormat,
            JoinedMatchSampleFactory.RivalNickname);
        MatchViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);
        PawnMovedEventArgs winningMove = PawnMovedSampleFactory.CreateWinningMove(
            JoinedMatchSampleFactory.RivalNickname,
            FirstGoalSquare);

        _matchClient.RaisePawnMoved(winningMove);

        Assert.Equal(expectedText, viewModel.TurnText);
    }

    [Fact]
    public void Constructor_ClassicBoard_PlacesPawnsOnStartingCoordinates()
    {
        var blueStart = new BoardCoordinates(ClassicCenterColumnIndex, FirstRowIndex);
        var redStart = new BoardCoordinates(ClassicCenterColumnIndex, ClassicLastRowIndex);
        var bluePawn = new BoardPawn(blueStart, PawnSymbol.Blue);
        var redPawn = new BoardPawn(redStart, PawnSymbol.Red);
        BoardPawn[] expectedPawns = { bluePawn, redPawn };
        MatchViewModelParts parts = CreateParts(Match.FirstTurnOrder);

        using var viewModel = new MatchViewModel(parts.JoinedMatch, parts.Abandonment, parts.MatchPlay);

        Assert.Equal(expectedPawns, viewModel.Pawns);
    }

    [Fact]
    public void Constructor_ClassicFormat_ShowsWallsPerPlayer()
    {
        string expectedText = string.Format(
            CultureInfo.CurrentCulture,
            Strings.MatchWallsFormat,
            JoinedMatchSampleFactory.WallsPerPlayer);
        MatchViewModelParts parts = CreateParts(Match.FirstTurnOrder);

        using var viewModel = new MatchViewModel(parts.JoinedMatch, parts.Abandonment, parts.MatchPlay);

        MatchPlayerViewModel firstPlayer = viewModel.Players[FirstPlayerIndex];
        Assert.Equal(expectedText, firstPlayer.WallsText);
    }

    [Fact]
    public void Constructor_FiveMinuteClock_ShowsFullClock()
    {
        MatchViewModelParts parts = CreateParts(Match.FirstTurnOrder);

        using var viewModel = new MatchViewModel(parts.JoinedMatch, parts.Abandonment, parts.MatchPlay);

        MatchPlayerViewModel firstPlayer = viewModel.Players[FirstPlayerIndex];
        Assert.Equal(FiveMinuteClockText, firstPlayer.ClockText);
    }

    [Fact]
    public void Constructor_FourPlayersOwnThirdTurn_ShowsFirstPlayerTurn()
    {
        string expectedText = string.Format(
            CultureInfo.CurrentCulture,
            Strings.MatchPlayerTurnFormat,
            JoinedMatchSampleFactory.RivalNickname);
        MatchViewModelParts parts = CreateFourPlayerParts();

        using var viewModel = new MatchViewModel(parts.JoinedMatch, parts.Abandonment, parts.MatchPlay);

        Assert.Equal(expectedText, viewModel.TurnText);
    }

    [Fact]
    public void Constructor_FourPlayers_PlacesFourPawns()
    {
        MatchViewModelParts parts = CreateFourPlayerParts();

        using var viewModel = new MatchViewModel(parts.JoinedMatch, parts.Abandonment, parts.MatchPlay);

        Assert.Equal(FourPlayers, viewModel.Pawns.Count);
    }

    [Fact]
    public void MatchAbandoned_MatchGoesOn_RemovesPawnOfLeftPlayer()
    {
        MatchViewModel viewModel = CreateFourPlayerViewModel();

        _matchClient.RaiseMatchAbandoned(
            JoinedMatchSampleFactory.MatchId,
            JoinedMatchSampleFactory.ThirdRivalNickname,
            isMatchOver: false);

        IEnumerable<PawnSymbol> symbols = viewModel.Pawns.Select(pawn => pawn.Symbol);
        Assert.Equal(_symbolsWithoutFourthPlayer, symbols);
    }

    [Fact]
    public void MatchAbandoned_MatchGoesOn_MarksLeftPlayerRow()
    {
        MatchViewModel viewModel = CreateFourPlayerViewModel();

        _matchClient.RaiseMatchAbandoned(
            JoinedMatchSampleFactory.MatchId,
            JoinedMatchSampleFactory.ThirdRivalNickname,
            isMatchOver: false);

        MatchPlayerViewModel fourthPlayer = viewModel.Players[FourthPlayerIndex];
        Assert.Equal(Strings.MatchPlayerLeft, fourthPlayer.DetailText);
    }

    [Fact]
    public void Constructor_PlayerLeftDuringVersus_StartsWithoutLeftPawn()
    {
        MatchViewModelParts parts = CreateFourPlayerParts();
        _matchClient.RaiseMatchAbandoned(
            JoinedMatchSampleFactory.MatchId,
            JoinedMatchSampleFactory.ThirdRivalNickname,
            isMatchOver: false);

        using var viewModel = new MatchViewModel(parts.JoinedMatch, parts.Abandonment, parts.MatchPlay);

        IEnumerable<PawnSymbol> symbols = viewModel.Pawns.Select(pawn => pawn.Symbol);
        Assert.Equal(_symbolsWithoutFourthPlayer, symbols);
    }

    private static void DisposeViewModel(IDisposable viewModel)
    {
        viewModel.Dispose();
    }

    private MatchViewModel CreateViewModel(byte ownTurnOrder)
    {
        MatchViewModelParts parts = CreateParts(ownTurnOrder);
        return CreateRegisteredViewModel(parts);
    }

    private MatchViewModelParts CreateParts(byte ownTurnOrder)
    {
        JoinedMatch joinedMatch = JoinedMatchSampleFactory.CreateWithOwnTurnOrder(ownTurnOrder);
        return CreateParts(joinedMatch);
    }

    private MatchViewModelParts CreateParts(JoinedMatch joinedMatch)
    {
        MatchAbandonmentViewModel abandonment = CreateAbandonmentViewModel(joinedMatch);
        MatchPlayViewModel matchPlayViewModel = CreatePlayViewModel(joinedMatch);
        return new MatchViewModelParts(joinedMatch, abandonment, matchPlayViewModel);
    }

    private MatchAbandonmentViewModel CreateAbandonmentViewModel(JoinedMatch joinedMatch)
    {
        Func<MatchAbandonmentViewModel> createViewModel = () => new MatchAbandonmentViewModel(
            joinedMatch,
            _matchClient);
        MatchAbandonmentViewModel viewModel = InlineUiContext.CreateViewModel(createViewModel);
        _createdViewModels.Add(viewModel);
        return viewModel;
    }

    private MatchPlayViewModel CreatePlayViewModel(JoinedMatch joinedMatch)
    {
        Func<MatchPlayViewModel> createViewModel = () => new MatchPlayViewModel(
            joinedMatch,
            _matchClient,
            _timeProvider);
        MatchPlayViewModel viewModel = InlineUiContext.CreateViewModel(createViewModel);
        _createdViewModels.Add(viewModel);
        return viewModel;
    }

    private MatchViewModel CreateRegisteredViewModel(MatchViewModelParts parts)
    {
        var viewModel = new MatchViewModel(parts.JoinedMatch, parts.Abandonment, parts.MatchPlay);
        _createdViewModels.Add(viewModel);
        return viewModel;
    }

    private static PawnMovedEventArgs CreateRivalFirstMove()
    {
        return PawnMovedSampleFactory.CreateFirstMove(
            JoinedMatchSampleFactory.MatchId,
            JoinedMatchSampleFactory.RivalNickname,
            SecondRowSquare);
    }

    private MatchViewModel CreateFourPlayerViewModel()
    {
        MatchViewModelParts parts = CreateFourPlayerParts();
        return CreateRegisteredViewModel(parts);
    }

    private MatchViewModelParts CreateFourPlayerParts()
    {
        JoinedMatch joinedMatch = JoinedMatchSampleFactory.CreateFourPlayerWithOwnThirdTurn();
        return CreateParts(joinedMatch);
    }

    private sealed record MatchViewModelParts(
        JoinedMatch JoinedMatch,
        MatchAbandonmentViewModel Abandonment,
        MatchPlayViewModel MatchPlay);
}
