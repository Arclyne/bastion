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

public class TestMatchViewModel : IDisposable
{
    private const int ClassicCenterColumnIndex = 4;
    private const int FirstRowIndex = 0;
    private const int ClassicLastRowIndex = 8;
    private const string FiveMinuteClockText = "5:00";
    private const int FourPlayers = 4;
    private const int FourthPlayerIndex = 3;

    private static readonly byte[] _turnOrders = new byte[]
    {
        Match.FirstTurnOrder,
        Match.SecondTurnOrder,
    };

    private readonly FakeMatchClient _matchClient = new FakeMatchClient();
    private readonly JoinedMatch _fourPlayerMatch = JoinedMatchSamples.CreateFourPlayerWithOwnThirdTurn();
    private readonly List<MatchAbandonmentViewModel> _abandonmentViewModels = new List<MatchAbandonmentViewModel>();
    private readonly MatchAbandonmentViewModel _fourPlayerAbandonment;

    public TestMatchViewModel()
    {
        _fourPlayerAbandonment = CreateAbandonmentViewModel(_fourPlayerMatch);
    }

    [Fact]
    public void Constructor_RivalListedFirst_OrdersPlayersByTurnOrder()
    {
        MatchViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);

        Assert.Equal(_turnOrders, viewModel.Players.Select(player => player.TurnOrder));
    }

    [Fact]
    public void Constructor_RivalListedFirst_MarksOwnPlayer()
    {
        MatchViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);

        Assert.Equal(JoinedMatchSamples.OwnNickname, viewModel.Players.Single(player => player.IsOwn).Nickname);
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

        Assert.True(viewModel.Players[0].HasFirstTurn());
    }

    [Fact]
    public void Constructor_ClassicBoard_PlacesPawnsOnStartingCoordinates()
    {
        var expectedPawns = new BoardPawn[]
        {
            new BoardPawn(new BoardCoordinates(ClassicCenterColumnIndex, FirstRowIndex), PawnSymbol.Blue),
            new BoardPawn(new BoardCoordinates(ClassicCenterColumnIndex, ClassicLastRowIndex), PawnSymbol.Red),
        };

        MatchViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);

        Assert.Equal(expectedPawns, viewModel.Pawns);
    }

    [Fact]
    public void Constructor_ClassicFormat_ShowsWallsPerPlayer()
    {
        string expectedText = string.Format(
            CultureInfo.CurrentCulture,
            Strings.MatchWallsFormat,
            JoinedMatchSamples.WallsPerPlayer);

        MatchViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);

        Assert.Equal(expectedText, viewModel.Players[0].WallsText);
    }

    [Fact]
    public void Constructor_FiveMinuteClock_ShowsFullClock()
    {
        MatchViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);

        Assert.Equal(FiveMinuteClockText, viewModel.Players[0].ClockText);
    }

    [Fact]
    public void Constructor_FourPlayersOwnThirdTurn_ShowsFirstPlayerTurn()
    {
        string expectedText = string.Format(
            CultureInfo.CurrentCulture,
            Strings.MatchPlayerTurnFormat,
            JoinedMatchSamples.RivalNickname);

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
    public void PlayerLeft_MatchGoesOn_RemovesPawnOfLeftPlayer()
    {
        MatchViewModel viewModel = CreateFourPlayerViewModel();

        _matchClient.RaiseMatchAbandoned(JoinedMatchSamples.MatchId, JoinedMatchSamples.FourthNickname, false);

        Assert.DoesNotContain(viewModel.Pawns, pawn => pawn.Symbol == PawnSymbol.Yellow);
    }

    [Fact]
    public void PlayerLeft_MatchGoesOn_MarksLeftPlayerRow()
    {
        MatchViewModel viewModel = CreateFourPlayerViewModel();

        _matchClient.RaiseMatchAbandoned(JoinedMatchSamples.MatchId, JoinedMatchSamples.FourthNickname, false);

        Assert.Equal(Strings.MatchPlayerLeft, viewModel.Players[FourthPlayerIndex].DetailText);
    }

    [Fact]
    public void Constructor_PlayerLeftDuringVersus_StartsWithoutLeftPawn()
    {
        _matchClient.RaiseMatchAbandoned(JoinedMatchSamples.MatchId, JoinedMatchSamples.FourthNickname, false);

        MatchViewModel viewModel = CreateFourPlayerViewModel();

        Assert.Equal(FourPlayers - 1, viewModel.Pawns.Count);
    }

    public void Dispose()
    {
        _abandonmentViewModels.ForEach(DisposeViewModel);
    }

    private static void DisposeViewModel(MatchAbandonmentViewModel viewModel)
    {
        viewModel.Dispose();
    }

    private MatchViewModel CreateViewModel(byte ownTurnOrder)
    {
        JoinedMatch joinedMatch = JoinedMatchSamples.CreateWithOwnTurnOrder(ownTurnOrder);
        return new MatchViewModel(joinedMatch, CreateAbandonmentViewModel(joinedMatch));
    }

    private MatchViewModel CreateFourPlayerViewModel()
    {
        return new MatchViewModel(_fourPlayerMatch, _fourPlayerAbandonment);
    }

    private MatchAbandonmentViewModel CreateAbandonmentViewModel(JoinedMatch joinedMatch)
    {
        var viewModel = new MatchAbandonmentViewModel(joinedMatch, _matchClient);
        _abandonmentViewModels.Add(viewModel);
        return viewModel;
    }
}
