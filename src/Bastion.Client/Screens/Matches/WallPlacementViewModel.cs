using System;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Domain.Matches;
using Gum.Mvvm;

namespace Bastion.Client.Screens.Matches;

public sealed class WallPlacementViewModel : ViewModel, IDisposable
{
    private readonly MatchPlayViewModel _matchPlayViewModel;
    private WallOrientation _orientation;

    public WallPlacementViewModel(MatchPlayViewModel matchPlayViewModel)
    {
        ArgumentNullException.ThrowIfNull(matchPlayViewModel);

        _matchPlayViewModel = matchPlayViewModel;
        _orientation = WallOrientation.Horizontal;
        HintText = string.Empty;
        CanStart = matchPlayViewModel.CanPlaceWall();
        _matchPlayViewModel.StateChanged += HandleMatchPlayStateChanged;
    }

    public bool IsActive
    {
        get => Get<bool>();
        private set => Set(value);
    }

    [DependsOn(nameof(IsActive))]
    public bool IsIdle => !IsActive;

    public bool CanStart
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public Wall? PendingWall
    {
        get => Get<Wall?>();
        private set => Set(value);
    }

    [DependsOn(nameof(PendingWall))]
    public bool HasPendingWall => PendingWall is not null;

    public string HintText
    {
        get => Get<string>();
        private set => Set(value);
    }

    [DependsOn(nameof(HintText))]
    public bool HasHint => HintText.Length > 0;

    public Wall? HoveredWall { get; private set; }

    public bool IsHoveredWallValid { get; private set; }

    public void Start()
    {
        if (!_matchPlayViewModel.CanPlaceWall())
        {
            return;
        }

        _matchPlayViewModel.ClearSelection();
        PendingWall = null;
        HoveredWall = null;
        HintText = Strings.MatchWallChooseHint;
        IsActive = true;
    }

    public void Cancel()
    {
        IsActive = false;
        PendingWall = null;
        HoveredWall = null;
        HintText = string.Empty;
    }

    public void Rotate()
    {
        if (!IsActive)
        {
            return;
        }

        _orientation = _orientation == WallOrientation.Horizontal
            ? WallOrientation.Vertical
            : WallOrientation.Horizontal;
        if (PendingWall is not null)
        {
            ChooseGroove(PendingWall.Groove);
            return;
        }

        if (HoveredWall is not null)
        {
            ShowHoveredWall(HoveredWall.Groove);
        }
    }

    public void HoverGroove(BoardCoordinates groove)
    {
        ArgumentNullException.ThrowIfNull(groove);

        bool isSameGroove = HoveredWall is not null && HoveredWall.Groove == groove;
        if (!IsActive || PendingWall is not null || isSameGroove)
        {
            return;
        }

        ShowHoveredWall(groove);
    }

    public void ClearHover()
    {
        HoveredWall = null;
    }

    public void ChooseGroove(BoardCoordinates groove)
    {
        ArgumentNullException.ThrowIfNull(groove);

        if (!IsActive)
        {
            return;
        }

        var wall = new Wall(groove, _orientation);
        WallRejection rejection = FindRejection(wall);
        if (rejection.Reason != WallRejectionReason.None)
        {
            PendingWall = null;
            ShowHoveredWall(wall, rejection);
            return;
        }

        HoveredWall = null;
        PendingWall = wall;
        HintText = Strings.MatchWallPendingHint;
    }

    public async Task Confirm()
    {
        if (PendingWall is not Wall wall)
        {
            return;
        }

        await _matchPlayViewModel.PlaceWall(wall);
        if (!_matchPlayViewModel.HasNotice)
        {
            return;
        }

        PendingWall = null;
        HintText = Strings.MatchWallChooseHint;
    }

    public void Dispose()
    {
        _matchPlayViewModel.StateChanged -= HandleMatchPlayStateChanged;
    }

    private void HandleMatchPlayStateChanged(object? sender, EventArgs e)
    {
        CanStart = _matchPlayViewModel.CanPlaceWall();
        if (IsActive && !_matchPlayViewModel.IsWallTurn())
        {
            Cancel();
        }
    }

    private void ShowHoveredWall(BoardCoordinates groove)
    {
        var wall = new Wall(groove, _orientation);
        WallRejection rejection = FindRejection(wall);
        ShowHoveredWall(wall, rejection);
    }

    private WallRejection FindRejection(Wall wall)
    {
        Board board = _matchPlayViewModel.BuildBoard();
        WallRejection rejection = WallPolicy.FindRejection(board, wall);
        return rejection;
    }

    private void ShowHoveredWall(Wall wall, WallRejection rejection)
    {
        bool isWallValid = rejection.Reason == WallRejectionReason.None;
        HoveredWall = wall;
        IsHoveredWallValid = isWallValid;
        HintText = isWallValid
            ? Strings.MatchWallChooseHint
            : _matchPlayViewModel.DescribeWallRejection(rejection.Reason, rejection.BlockedTurnOrder);
    }
}
