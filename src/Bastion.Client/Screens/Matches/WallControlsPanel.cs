using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
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
        Label hintLabel = CreateHint();
        StackPanel buttonRowStackPanel = CreateButtonRow();
        AddChild(hintLabel);
        AddChild(buttonRowStackPanel);
    }

    private static Label CreateHint()
    {
        Label hintLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(WallPlacementViewModel.HintText),
            TextStyleCatalog.IdentityDetail,
            HintWidth);
        hintLabel.SetBinding(nameof(IsVisible), nameof(WallPlacementViewModel.HasHint));
        return hintLabel;
    }

    private StackPanel CreateButtonRow()
    {
        Button placeWallButton = CreateWallButton(nameof(Strings.MatchPlaceWallButton), ButtonLookCatalog.Pill);
        placeWallButton.SetBinding(nameof(IsEnabled), nameof(WallPlacementViewModel.CanStart));
        placeWallButton.SetBinding(nameof(IsVisible), nameof(WallPlacementViewModel.IsIdle));
        placeWallButton.Click += HandleStartClick;

        Button rotateButton = CreateWallButton(nameof(Strings.MatchRotateWallButton), ButtonLookCatalog.Secondary);
        rotateButton.SetBinding(nameof(IsVisible), nameof(WallPlacementViewModel.IsActive));
        rotateButton.Click += HandleRotateClick;

        Button confirmButton = CreateWallButton(nameof(Strings.MatchConfirmWallButton), ButtonLookCatalog.Primary);
        confirmButton.SetBinding(nameof(IsVisible), nameof(WallPlacementViewModel.IsActive));
        confirmButton.SetBinding(nameof(IsEnabled), nameof(WallPlacementViewModel.HasPendingWall));
        confirmButton.Click += HandleConfirmClick;

        Button cancelButton = CreateWallButton(nameof(Strings.MatchCancelWallButton), ButtonLookCatalog.Secondary);
        cancelButton.SetBinding(nameof(IsVisible), nameof(WallPlacementViewModel.IsActive));
        cancelButton.Click += HandleCancelClick;

        var rowStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = ButtonSpacing,
        };
        rowStackPanel.AddChild(placeWallButton);
        rowStackPanel.AddChild(rotateButton);
        rowStackPanel.AddChild(confirmButton);
        rowStackPanel.AddChild(cancelButton);
        return rowStackPanel;
    }

    private static Button CreateWallButton(string textKey, ButtonLook buttonLook)
    {
        Button button = ViewFactory.CreateButton(textKey, ButtonWidth, ScreenMetrics.MenuPillHeight);
        ButtonSkin.CreateAndAttach(button, buttonLook);
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
