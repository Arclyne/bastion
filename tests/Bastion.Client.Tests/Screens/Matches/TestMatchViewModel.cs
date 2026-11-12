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
    private const int ClassicLastRowIndex = 8;
    private const string FiveMinuteClockText = "5:00";
    private const int FourPlayers = 4;
    private const int FourthPlayerIndex = 3;

    private static readonly BoardCoordinates _rivalForward = new BoardCoordinates(ClassicCenterColumnIndex, SecondRowIndex);
    private static readonly byte[] _turnOrders = { Match.FirstTurnOrder, Match.SecondTurnOrder };
    private static readonly PawnSymbol[] _symbolsWithoutFourthPlayer =
    {
        PawnSymbol.Blue,
        PawnSymbol.Red,
        PawnSymbol.Green,
    };

    private readonly FakeMatchClient _matchClient = new FakeMatchClient();
    private readonly List<IDisposable> _companionViewModels = new List<IDisposable>();

    public void Dispose()
    {
        _companionViewModels.ForEach(DisposeViewModel);
    }

    [Fact]
    public void Constructor_RivalListedFirst_OrdersPlayersByTurnOrder()
    {
        MatchViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);

        IEnumerable<byte> turnOrders = viewModel.Players.Select(player => player.TurnOrder);
        Assert.Equal(_turnOrders, turnOrders);
    }

    [Fact]
    public void Constructor_RivalListedFirst_MarksOwnPlayer()
    {
        MatchViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);

        MatchPlayerViewModel ownPlayer = viewModel.Players.Single(player => player.IsOwn);
        Assert.Equal(JoinedMatchSampleFactory.OwnNickname, ownPlayer.Nickname);
    }

    [Fact]
    public void Constructor_OwnSecondTurnOrder_KeepsOwnTurnOrder()
    {
        MatchViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);

        Assert.Equal(Match.SecondTurnOrder, viewModel.OwnTurnOrder);
    }

    [Fact]
    public void Constructor_OwnFirstTurnOrder_ShowsOwnTurn()
    {
        MatchViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);

        Assert.Equal(Strings.MatchYourTurn, viewModel.TurnText);
    }

    [Fact]
    public void Constructor_OwnSecondTurnOrder_ShowsRivalTurn()
    {
        MatchViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);

        Assert.Equal(Strings.MatchRivalTurn, viewModel.TurnText);
    }

    [Fact]
    public void Constructor_FirstTurnOrderPlayer_HasTurn()
    {
        MatchViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);

        MatchPlayerViewModel firstPlayer = viewModel.Players[0];
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

        MatchPlayerViewModel ownPlayer = viewModel.Players[1];
        Assert.True(ownPlayer.HasTurn);
    }

    [Fact]
    public void PawnMoved_RivalMoved_MovesRivalPawn()
    {
        MatchViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);
        PawnMovedEventArgs rivalMove = CreateRivalFirstMove();
        var expectedPawn = new BoardPawn(_rivalForward, PawnSymbol.Blue);

        _matchClient.RaisePawnMoved(rivalMove);

        Assert.Contains(expectedPawn, viewModel.Pawns);
    }

    [Fact]
    public void Constructor_RivalMovedDuringVersus_StartsWithMovedPawn()
    {
        JoinedMatch joinedMatch = JoinedMatchSampleFactory.CreateWithOwnTurnOrder(Match.SecondTurnOrder);
        MatchAbandonmentViewModel abandonment = CreateAbandonmentViewModel(joinedMatch);
        MatchPlayViewModel play = CreatePlayViewModel(joinedMatch);
        PawnMovedEventArgs rivalMove = CreateRivalFirstMove();
        _matchClient.RaisePawnMoved(rivalMove);
        var expectedPawn = new BoardPawn(_rivalForward, PawnSymbol.Blue);

        var viewModel = new MatchViewModel(joinedMatch, abandonment, play);

        Assert.Contains(expectedPawn, viewModel.Pawns);
    }

    [Fact]
    public void PawnMoved_OwnPawnReachedGoal_ShowsOwnWin()
    {
        MatchViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);
        PawnMovedEventArgs winningMove =
            PawnMovedSamples.CreateWinningMove(JoinedMatchSampleFactory.OwnNickname, FirstGoalSquare);

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
        PawnMovedEventArgs winningMove =
            PawnMovedSamples.CreateWinningMove(JoinedMatchSampleFactory.RivalNickname, FirstGoalSquare);

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

        MatchViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);

        Assert.Equal(expectedPawns, viewModel.Pawns);
    }

    [Fact]
    public void Constructor_ClassicFormat_ShowsWallsPerPlayer()
    {
        string expectedText = string.Format(
            CultureInfo.CurrentCulture,
            Strings.MatchWallsFormat,
            JoinedMatchSampleFactory.WallsPerPlayer);

        MatchViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);

        MatchPlayerViewModel firstPlayer = viewModel.Players[0];
        Assert.Equal(expectedText, firstPlayer.WallsText);
    }

    [Fact]
    public void Constructor_FiveMinuteClock_ShowsFullClock()
    {
        MatchViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);

        MatchPlayerViewModel firstPlayer = viewModel.Players[0];
        Assert.Equal(FiveMinuteClockText, firstPlayer.ClockText);
    }

    [Fact]
    public void Constructor_FourPlayersOwnThirdTurn_ShowsFirstPlayerTurn()
    {
        string expectedText = string.Format(
            CultureInfo.CurrentCulture,
            Strings.MatchPlayerTurnFormat,
            JoinedMatchSampleFactory.RivalNickname);

        MatchViewModel viewModel = CreateFourPlayerViewModel();

        Assert.Equal(expectedText, viewModel.TurnText);
    }

    [Fact]
    public void Constructor_FourPlayers_PlacesFourPawns()
    {
        MatchViewModel viewModel = CreateFourPlayerViewModel();

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
        JoinedMatch joinedMatch = JoinedMatchSampleFactory.CreateFourPlayerWithOwnThirdTurn();
        MatchAbandonmentViewModel abandonment = CreateAbandonmentViewModel(joinedMatch);
        MatchPlayViewModel play = CreatePlayViewModel(joinedMatch);
        _matchClient.RaiseMatchAbandoned(
            JoinedMatchSampleFactory.MatchId,
            JoinedMatchSampleFactory.ThirdRivalNickname,
            isMatchOver: false);
        int expectedPawnCount = FourPlayers - 1;

        var viewModel = new MatchViewModel(joinedMatch, abandonment, play);

        Assert.Equal(expectedPawnCount, viewModel.Pawns.Count);
    }

    private static void DisposeViewModel(IDisposable viewModel)
    {
        viewModel.Dispose();
    }

    private MatchViewModel CreateViewModel(byte ownTurnOrder)
    {
        JoinedMatch joinedMatch = JoinedMatchSampleFactory.CreateWithOwnTurnOrder(ownTurnOrder);
        MatchAbandonmentViewModel abandonment = CreateAbandonmentViewModel(joinedMatch);
        MatchPlayViewModel play = CreatePlayViewModel(joinedMatch);
        return new MatchViewModel(joinedMatch, abandonment, play);
    }

    private MatchAbandonmentViewModel CreateAbandonmentViewModel(JoinedMatch joinedMatch)
    {
        var viewModel = new MatchAbandonmentViewModel(joinedMatch, _matchClient);
        _companionViewModels.Add(viewModel);
        return viewModel;
    }

    private MatchPlayViewModel CreatePlayViewModel(JoinedMatch joinedMatch)
    {
        var viewModel = new MatchPlayViewModel(joinedMatch, _matchClient);
        _companionViewModels.Add(viewModel);
        return viewModel;
    }

    private static PawnMovedEventArgs CreateRivalFirstMove()
    {
        return PawnMovedSamples.CreateFirstMove(
            JoinedMatchSampleFactory.MatchId,
            JoinedMatchSampleFactory.RivalNickname,
            SecondRowSquare);
    }

    private MatchViewModel CreateFourPlayerViewModel()
    {
        JoinedMatch joinedMatch = JoinedMatchSampleFactory.CreateFourPlayerWithOwnThirdTurn();
        MatchAbandonmentViewModel abandonment = CreateAbandonmentViewModel(joinedMatch);
        MatchPlayViewModel play = CreatePlayViewModel(joinedMatch);
        return new MatchViewModel(joinedMatch, abandonment, play);
    }
}
