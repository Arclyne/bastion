using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Matches;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Screens.Matches;
using Bastion.Client.Tests.Fakes;
using Bastion.Domain.Matches;
using Xunit;

namespace Bastion.Client.Tests.Screens.Matches;

public sealed class TestWallPlacementViewModel : IDisposable
{
    private const int CenterColumnIndex = 4;
    private const int FourthRowIndex = 3;
    private const int LastColumnIndex = 8;
    private const int FirstRowIndex = 0;
    private const string CenterGroove = "e4";

    private static readonly BoardCoordinates _centerGroove = new BoardCoordinates(CenterColumnIndex, FourthRowIndex);
    private static readonly BoardCoordinates _lastColumnGroove = new BoardCoordinates(LastColumnIndex, FirstRowIndex);
    private static readonly Wall _centerHorizontalWall = new Wall(_centerGroove, WallOrientation.Horizontal);
    private static readonly Wall _centerVerticalWall = new Wall(_centerGroove, WallOrientation.Vertical);

    private readonly FakeMatchClient _matchClient = new FakeMatchClient();
    private readonly List<IDisposable> _viewModels = new List<IDisposable>();

    public void Dispose()
    {
        _viewModels.ForEach(DisposeViewModel);
    }

    [Fact]
    public void Start_OwnTurn_Activates()
    {
        WallPlacementViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);

        viewModel.Start();

        Assert.True(viewModel.IsActive);
    }

    [Fact]
    public void Start_RivalTurn_StaysIdle()
    {
        WallPlacementViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);

        viewModel.Start();

        Assert.False(viewModel.IsActive);
    }

    [Fact]
    public void HoverGroove_FreeGroove_MarksWallValid()
    {
        WallPlacementViewModel viewModel = CreateActiveViewModel();

        viewModel.HoverGroove(_centerGroove);

        Assert.True(viewModel.IsHoveredWallValid);
    }

    [Fact]
    public void HoverGroove_GrooveOnLastColumn_ShowsOutOfBoard()
    {
        WallPlacementViewModel viewModel = CreateActiveViewModel();

        viewModel.HoverGroove(_lastColumnGroove);

        Assert.Equal(Strings.MatchWallOutOfBoard, viewModel.HintText);
    }

    [Fact]
    public void ChooseGroove_FreeGroove_KeepsPendingWall()
    {
        WallPlacementViewModel viewModel = CreateActiveViewModel();

        viewModel.ChooseGroove(_centerGroove);

        Assert.Equal(_centerHorizontalWall, viewModel.PendingWall);
    }

    [Fact]
    public void ChooseGroove_GrooveTaken_ShowsGrooveTaken()
    {
        WallPlacementViewModel viewModel = CreateViewModel(Match.SecondTurnOrder);
        WallPlacedEventArgs rivalWall =
            WallPlacedSampleFactory.CreateFirstWall(JoinedMatchSampleFactory.RivalNickname, CenterGroove);
        _matchClient.RaiseWallPlaced(rivalWall);
        viewModel.Start();

        viewModel.ChooseGroove(_centerGroove);

        Assert.Equal(Strings.MatchWallGrooveTaken, viewModel.HintText);
    }

    [Fact]
    public void Rotate_PendingWall_TurnsPendingWallVertical()
    {
        WallPlacementViewModel viewModel = CreateActiveViewModel();
        viewModel.ChooseGroove(_centerGroove);

        viewModel.Rotate();

        Assert.Equal(_centerVerticalWall, viewModel.PendingWall);
    }

    [Fact]
    public async Task Confirm_PendingWall_SendsWall()
    {
        WallPlacementViewModel viewModel = CreateActiveViewModel();
        viewModel.ChooseGroove(_centerGroove);

        await viewModel.Confirm();

        Assert.Equal(CenterGroove, _matchClient.LastWall?.Groove);
    }

    [Fact]
    public async Task Confirm_Accepted_LeavesWallMode()
    {
        WallPlacementViewModel viewModel = CreateActiveViewModel();
        viewModel.ChooseGroove(_centerGroove);

        await viewModel.Confirm();

        Assert.False(viewModel.IsActive);
    }

    [Fact]
    public async Task Confirm_Rejected_StaysInWallMode()
    {
        WallPlacementViewModel viewModel = CreateActiveViewModel();
        viewModel.ChooseGroove(_centerGroove);
        var cause = new InvalidOperationException();
        var grooveTaken = new IllegalWallException(WallPlacementRejection.GrooveTaken, blockedTurnOrder: null, cause);
        _matchClient.WallFailure = grooveTaken;

        await viewModel.Confirm();

        Assert.True(viewModel.IsActive);
    }

    [Fact]
    public void Cancel_PendingWall_ClearsPendingWall()
    {
        WallPlacementViewModel viewModel = CreateActiveViewModel();
        viewModel.ChooseGroove(_centerGroove);

        viewModel.Cancel();

        Assert.Null(viewModel.PendingWall);
    }

    [Fact]
    public void WallPlaced_TurnPassed_LeavesWallMode()
    {
        WallPlacementViewModel viewModel = CreateActiveViewModel();
        WallPlacedEventArgs ownWall =
            WallPlacedSampleFactory.CreateFirstWall(JoinedMatchSampleFactory.OwnNickname, CenterGroove);

        _matchClient.RaiseWallPlaced(ownWall);

        Assert.False(viewModel.IsActive);
    }

    private static void DisposeViewModel(IDisposable viewModel)
    {
        viewModel.Dispose();
    }

    private WallPlacementViewModel CreateActiveViewModel()
    {
        WallPlacementViewModel viewModel = CreateViewModel(Match.FirstTurnOrder);
        viewModel.Start();
        return viewModel;
    }

    private WallPlacementViewModel CreateViewModel(byte ownTurnOrder)
    {
        JoinedMatch joinedMatch = JoinedMatchSampleFactory.CreateWithOwnTurnOrder(ownTurnOrder);
        MatchPlayViewModel play =
            InlineUiContext.CreateViewModel(() => new MatchPlayViewModel(joinedMatch, _matchClient));
        var viewModel = new WallPlacementViewModel(play);
        _viewModels.Add(viewModel);
        _viewModels.Add(play);
        return viewModel;
    }
}
