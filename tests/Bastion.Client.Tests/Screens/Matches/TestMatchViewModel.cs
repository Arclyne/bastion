using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Bastion.Client.Localization;
using Bastion.Client.Screens.Matches;
using Bastion.Client.Tests.Fakes;
using Bastion.Domain.Matches;
using Xunit;

namespace Bastion.Client.Tests.Screens.Matches;

public sealed class TestMatchViewModel : IDisposable
{
    private const int ClassicCenterColumnIndex = 4;
    private const int FirstRowIndex = 0;
    private const int ClassicLastRowIndex = 8;
    private const string FiveMinuteClockText = "5:00";
    private const int FourPlayers = 4;
    private const int FourthPlayerIndex = 3;

    private static readonly byte[] _turnOrders = { Match.FirstTurnOrder, Match.SecondTurnOrder };
    private static readonly PawnSymbol[] _symbolsWithoutFourthPlayer =
    {
        PawnSymbol.Blue,
        PawnSymbol.Red,
        PawnSymbol.Green,
    };

    private readonly FakeMatchClient _matchClient = new FakeMatchClient();
    private readonly List<MatchAbandonmentViewModel> _abandonmentViewModels = new List<MatchAbandonmentViewModel>();

    public void Dispose()
    {
        _abandonmentViewModels.ForEach(DisposeViewModel);
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
    public void Constructor_FirstTurnOrderPlayer_HasFirstTurn()
    {
        MatchViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);

        MatchPlayerViewModel firstPlayer = viewModel.Players[0];
        bool hasFirstTurn = firstPlayer.HasFirstTurn();
        Assert.True(hasFirstTurn);
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
        _matchClient.RaiseMatchAbandoned(
            JoinedMatchSampleFactory.MatchId,
            JoinedMatchSampleFactory.ThirdRivalNickname,
            isMatchOver: false);
        int expectedPawnCount = FourPlayers - 1;

        var viewModel = new MatchViewModel(joinedMatch, abandonment);

        Assert.Equal(expectedPawnCount, viewModel.Pawns.Count);
    }

    private static void DisposeViewModel(MatchAbandonmentViewModel viewModel)
    {
        viewModel.Dispose();
    }

    private MatchViewModel CreateViewModel(byte ownTurnOrder)
    {
        JoinedMatch joinedMatch = JoinedMatchSampleFactory.CreateWithOwnTurnOrder(ownTurnOrder);
        MatchAbandonmentViewModel abandonment = CreateAbandonmentViewModel(joinedMatch);
        return new MatchViewModel(joinedMatch, abandonment);
    }

    private MatchAbandonmentViewModel CreateAbandonmentViewModel(JoinedMatch joinedMatch)
    {
        var viewModel = new MatchAbandonmentViewModel(joinedMatch, _matchClient);
        _abandonmentViewModels.Add(viewModel);
        return viewModel;
    }

    private MatchViewModel CreateFourPlayerViewModel()
    {
        JoinedMatch joinedMatch = JoinedMatchSampleFactory.CreateFourPlayerWithOwnThirdTurn();
        MatchAbandonmentViewModel abandonment = CreateAbandonmentViewModel(joinedMatch);
        return new MatchViewModel(joinedMatch, abandonment);
    }
}
