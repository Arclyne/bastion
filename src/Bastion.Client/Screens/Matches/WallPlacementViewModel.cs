using System;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Domain.Matches;
using Gum.Mvvm;

namespace Bastion.Client.Screens.Matches;

public sealed class WallPlacementViewModel : ViewModel, IDisposable
{
    private readonly MatchPlayViewModel _play;

    public WallPlacementViewModel(MatchPlayViewModel play)
    {
        ArgumentNullException.ThrowIfNull(play);

        _play = play;
        Orientation = WallOrientation.Horizontal;
        HintText = string.Empty;
        CanStart = play.CanPlaceWall();
        _play.StateChanged += HandlePlayStateChanged;
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

    public WallOrientation Orientation { get; private set; }

    public Wall? HoveredWall { get; private set; }

    public bool IsHoveredWallValid { get; private set; }

    public void Start()
    {
        if (!_play.CanPlaceWall())
        {
            return;
        }

        _play.ClearSelection();
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

        Orientation = Orientation == WallOrientation.Horizontal ? WallOrientation.Vertical : WallOrientation.Horizontal;
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

        var wall = new Wall(groove, Orientation);
        WallRejection? rejection = FindRejection(wall);
        if (rejection is not null)
        {
            PendingWall = null;
            HoveredWall = wall;
            IsHoveredWallValid = false;
            HintText = _play.DescribeWallRejection(rejection.Reason, rejection.BlockedTurnOrder);
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

        await _play.PlaceWall(wall);
        if (!_play.HasNotice)
        {
            return;
        }

        PendingWall = null;
        HintText = Strings.MatchWallChooseHint;
    }

    public void Dispose()
    {
        _play.StateChanged -= HandlePlayStateChanged;
    }

    private void HandlePlayStateChanged(object? sender, EventArgs e)
    {
        CanStart = _play.CanPlaceWall();
        if (IsActive && !_play.IsWallTurn())
        {
            Cancel();
        }
    }

    private void ShowHoveredWall(BoardCoordinates groove)
    {
        var wall = new Wall(groove, Orientation);
        WallRejection? rejection = FindRejection(wall);
        HoveredWall = wall;
        IsHoveredWallValid = rejection is null;
        HintText = rejection is null
            ? Strings.MatchWallChooseHint
            : _play.DescribeWallRejection(rejection.Reason, rejection.BlockedTurnOrder);
    }

    private WallRejection? FindRejection(Wall wall)
    {
        Board board = _play.BuildBoard();
        WallRejection? rejection = WallPolicy.FindRejection(board, wall);
        return rejection;
    }
}
