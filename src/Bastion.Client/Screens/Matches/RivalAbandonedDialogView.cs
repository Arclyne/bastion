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
        Label titleLabel = ViewFactory.CreateLabel(nameof(Strings.MatchOverTitle), TextStyleCatalog.DialogTitle);
        Label messageLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(MatchAbandonmentViewModel.RivalAbandonedText),
            TextStyleCatalog.DialogMessage,
            DialogMetrics.ContentWidth);
        Button menuButton = CreateWideButton(nameof(Strings.MatchBackToMenuButton), HandleMenuClick);
        Content.AddChild(titleLabel);
        Content.AddChild(messageLabel);
        Content.AddChild(menuButton);
    }

    private void HandleMenuClick(object? sender, EventArgs e)
    {
        _viewModel.RequestReturnToMenu();
    }
}
