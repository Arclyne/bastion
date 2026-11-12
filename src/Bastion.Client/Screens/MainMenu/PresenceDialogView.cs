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
        Content.AddChild(ViewFactory.CreateLabel(nameof(Strings.MatchSearchPresenceTitle), TextStyleCatalog.DialogTitle));
        Content.AddChild(ViewFactory.CreateBoundUntranslatedLabel(
            nameof(MatchSearchViewModel.PresenceMessage),
            TextStyleCatalog.DialogMessage,
            DialogMetrics.ContentWidth));
        Content.AddChild(CreateConfirmButton());
    }

    private Button CreateConfirmButton()
    {
        Button confirmButton = ViewFactory.CreateButton(
            nameof(Strings.MatchSearchPresenceConfirmButton),
            DialogMetrics.ContentWidth,
            DialogMetrics.ButtonHeight);
        ButtonSkin.CreateAndAttach(confirmButton, ButtonLookCatalog.Primary);
        confirmButton.Click += HandleConfirmClick;
        return confirmButton;
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
