using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.Forms.Controls;

namespace Bastion.Client.Screens.Matches;

public sealed class RivalAbandonedDialogView : DialogCard
{
    private readonly MatchAbandonmentViewModel _viewModel;

    public RivalAbandonedDialogView(MatchAbandonmentViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;
        Anchor(Gum.Wireframe.Anchor.Center);
        Label title = ViewFactory.CreateLabel(nameof(Strings.MatchOverTitle), TextStyleCatalog.DialogTitle);
        Label message = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(MatchAbandonmentViewModel.RivalAbandonedText),
            TextStyleCatalog.DialogMessage,
            DialogMetrics.ContentWidth);
        Button menuButton = CreateMenuButton();
        Content.AddChild(title);
        Content.AddChild(message);
        Content.AddChild(menuButton);
    }

    private Button CreateMenuButton()
    {
        Button menuButton = ViewFactory.CreateButton(
            nameof(Strings.MatchBackToMenuButton),
            DialogMetrics.ContentWidth,
            DialogMetrics.ButtonHeight);
        ButtonSkin.CreateAndAttach(menuButton, ButtonLookCatalog.Primary);
        menuButton.Click += HandleMenuClick;
        return menuButton;
    }

    private void HandleMenuClick(object? sender, EventArgs e)
    {
        _viewModel.RequestReturnToMenu();
    }
}
