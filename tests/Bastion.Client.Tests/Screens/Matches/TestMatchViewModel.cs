using System.Globalization;
using System.Linq;
using Bastion.Client.Localization;
using Bastion.Client.Screens.Matches;
using Bastion.Domain.Matches;
using Xunit;

namespace Bastion.Client.Tests.Screens.Matches;

public class TestMatchViewModel
{
    private const int ClassicCenterColumnIndex = 4;
    private const int FirstRowIndex = 0;
    private const int ClassicLastRowIndex = 8;
    private const string FiveMinuteClockText = "5:00";

    private static readonly byte[] _turnOrders = new byte[]
    {
        Match.FirstTurnOrder,
        Match.SecondTurnOrder,
    };

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
    public void Constructor_OwnFirstTurnOrder_KeepsBoardUnrotated()
    {
        MatchViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);

        Assert.False(viewModel.IsBoardRotated);
    }

    [Fact]
    public void Constructor_OwnSecondTurnOrder_RotatesBoard()
    {
        MatchViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);

        Assert.True(viewModel.IsBoardRotated);
    }

    [Fact]
    public void Constructor_OwnFirstTurnOrder_ShowsOwnTurn()
    {
        MatchViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);

        Assert.Equal(nameof(Strings.MatchYourTurn), viewModel.TurnTextKey);
    }

    [Fact]
    public void Constructor_OwnSecondTurnOrder_ShowsRivalTurn()
    {
        MatchViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);

        Assert.Equal(nameof(Strings.MatchRivalTurn), viewModel.TurnTextKey);
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

    private static MatchViewModel CreateViewModel(byte ownTurnOrder)
    {
        return new MatchViewModel(JoinedMatchSamples.CreateWithOwnTurnOrder(ownTurnOrder));
    }
}
