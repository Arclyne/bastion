using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using log4net;

namespace Bastion.Client.Screens.Matches;

public sealed class WallControlsPanel : StackPanel
{
    private const float ButtonWidth = 150;
    private const float ButtonSpacing = 10;
    private const float RowSpacing = 8;
    private const float HintWidth = 480;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(WallControlsPanel));

    private readonly WallPlacementViewModel _viewModel;

    public WallControlsPanel(WallPlacementViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;
        BindingContext = viewModel;
        Spacing = RowSpacing;
        AddChild(CreateHint());
        AddChild(CreateButtonRow());
    }

    private static Label CreateHint()
    {
        Label hint = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(WallPlacementViewModel.HintText),
            TextStyleCatalog.IdentityDetail,
            HintWidth);
        hint.SetBinding(nameof(IsVisible), nameof(WallPlacementViewModel.HasHint));
        return hint;
    }

    private StackPanel CreateButtonRow()
    {
        Button start = CreateWallButton(nameof(Strings.MatchPlaceWallButton), ButtonLookCatalog.Pill);
        start.SetBinding(nameof(IsEnabled), nameof(WallPlacementViewModel.CanStart));
        start.SetBinding(nameof(IsVisible), nameof(WallPlacementViewModel.IsIdle));
        start.Click += HandleStartClick;

        Button rotate = CreateWallButton(nameof(Strings.MatchRotateWallButton), ButtonLookCatalog.Secondary);
        rotate.SetBinding(nameof(IsVisible), nameof(WallPlacementViewModel.IsActive));
        rotate.Click += HandleRotateClick;

        Button confirm = CreateWallButton(nameof(Strings.MatchConfirmWallButton), ButtonLookCatalog.Primary);
        confirm.SetBinding(nameof(IsVisible), nameof(WallPlacementViewModel.IsActive));
        confirm.SetBinding(nameof(IsEnabled), nameof(WallPlacementViewModel.HasPendingWall));
        confirm.Click += HandleConfirmClick;

        Button cancel = CreateWallButton(nameof(Strings.MatchCancelWallButton), ButtonLookCatalog.Secondary);
        cancel.SetBinding(nameof(IsVisible), nameof(WallPlacementViewModel.IsActive));
        cancel.Click += HandleCancelClick;

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = ButtonSpacing,
        };
        row.AddChild(start);
        row.AddChild(rotate);
        row.AddChild(confirm);
        row.AddChild(cancel);
        return row;
    }

    private static Button CreateWallButton(string textKey, ButtonLook look)
    {
        Button button = ViewFactory.CreateButton(textKey, ButtonWidth, ScreenMetrics.MenuPillHeight);
        ButtonSkin.CreateAndAttach(button, look);
        return button;
    }

    private void HandleStartClick(object? sender, EventArgs e)
    {
        _viewModel.Start();
    }

    private void HandleRotateClick(object? sender, EventArgs e)
    {
        _viewModel.Rotate();
    }

    private async void HandleConfirmClick(object? sender, EventArgs e)
    {
        try
        {
            await _viewModel.Confirm();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The wall could not be placed because the connection is closed.", ex);
        }
    }

    private void HandleCancelClick(object? sender, EventArgs e)
    {
        _viewModel.Cancel();
    }
}
