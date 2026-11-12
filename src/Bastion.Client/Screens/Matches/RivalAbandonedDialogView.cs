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
        Content.AddChild(ViewFactory.CreateLabel(nameof(Strings.MatchOverTitle), TextStyles.DialogTitle));
        Content.AddChild(ViewFactory.CreateBoundUntranslatedLabel(
            nameof(MatchAbandonmentViewModel.RivalAbandonedText),
            TextStyles.DialogMessage,
            DialogMetrics.ContentWidth));
        Content.AddChild(CreateMenuButton());
    }

    private Button CreateMenuButton()
    {
        Button menuButton = ViewFactory.CreateButton(
            nameof(Strings.MatchBackToMenuButton),
            DialogMetrics.ContentWidth,
            DialogMetrics.ButtonHeight);
        ButtonSkin.CreateAndAttach(menuButton, ButtonLooks.Primary);
        menuButton.Click += HandleMenuClick;
        return menuButton;
    }

    private void HandleMenuClick(object? sender, EventArgs e)
    {
        _viewModel.RequestReturnToMenu();
    }
}
