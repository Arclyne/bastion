using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.Forms.Controls;

namespace Bastion.Client.Screens.Matches;

public sealed class MatchTakenOverDialogView : DialogCard
{
    private readonly MatchConnectionViewModel _viewModel;

    public MatchTakenOverDialogView(MatchConnectionViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;
        BindingContext = viewModel;
        Anchor(Gum.Wireframe.Anchor.Center);
        Label titleLabel = ViewFactory.CreateLabel(nameof(Strings.MatchTakenOverTitle), TextStyleCatalog.DialogTitle);
        Label messageLabel = ViewFactory.CreateLabel(
            nameof(Strings.MatchTakenOverMessage),
            TextStyleCatalog.DialogMessage);
        ViewFactory.SetFixedWidth(messageLabel, DialogMetrics.ContentWidth);
        Button menuButton = CreateWideButton(nameof(Strings.MatchGoToMenuButton), HandleMenuClick);
        Content.AddChild(titleLabel);
        Content.AddChild(messageLabel);
        Content.AddChild(menuButton);
    }

    private void HandleMenuClick(object? sender, EventArgs e)
    {
        _viewModel.GoToMenu();
    }
}
