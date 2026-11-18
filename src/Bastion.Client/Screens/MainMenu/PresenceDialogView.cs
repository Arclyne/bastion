using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.Forms.Controls;
using log4net;

namespace Bastion.Client.Screens.MainMenu;

public sealed class PresenceDialogView : DialogCard
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(PresenceDialogView));

    private readonly MatchSearchViewModel _viewModel;

    public PresenceDialogView(MatchSearchViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;
        Anchor(Gum.Wireframe.Anchor.Center);
        Label titleLabel = ViewFactory.CreateLabel(
            nameof(Strings.MatchSearchPresenceTitle),
            TextStyleCatalog.DialogTitle);
        Label messageLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(MatchSearchViewModel.PresenceMessage),
            TextStyleCatalog.DialogMessage,
            DialogMetrics.ContentWidth);
        Button confirmButton = CreateWideButton(nameof(Strings.MatchSearchPresenceConfirmButton), HandleConfirmClick);
        Content.AddChild(titleLabel);
        Content.AddChild(messageLabel);
        Content.AddChild(confirmButton);
    }

    private async void HandleConfirmClick(object? sender, EventArgs e)
    {
        try
        {
            await _viewModel.ConfirmPresence();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The presence could not be confirmed because the connection is closed.", ex);
        }
    }
}
