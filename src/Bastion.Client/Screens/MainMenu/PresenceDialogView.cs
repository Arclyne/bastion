using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using log4net;

namespace Bastion.Client.Screens.MainMenu;

public sealed class PresenceDialogView : Panel
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(PresenceDialogView));

    private readonly MatchSearchViewModel _viewModel;

    public PresenceDialogView(MatchSearchViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;
        Anchor(Gum.Wireframe.Anchor.Center);
        Width = DialogMetrics.Width;
        WidthUnits = DimensionUnitType.Absolute;
        Height = DialogMetrics.PaddingBottom;
        HeightUnits = DimensionUnitType.RelativeToChildren;
        AddChild(CreateContent());
        Skins.ApplyCard(Visual);
    }

    private StackPanel CreateContent()
    {
        Button confirmButton = ViewFactory.CreateButton(
            nameof(Strings.MatchSearchPresenceConfirmButton),
            DialogMetrics.ContentWidth,
            DialogMetrics.ButtonHeight);
        ButtonSkin.CreateAndAttach(confirmButton, ButtonLooks.Primary);
        confirmButton.Click += HandleConfirmClick;

        var content = new StackPanel
        {
            X = DialogMetrics.PaddingX,
            Y = DialogMetrics.PaddingTop,
            Spacing = DialogMetrics.SectionSpacing,
        };
        content.AddChild(ViewFactory.CreateLabel(nameof(Strings.MatchSearchPresenceTitle), TextStyles.DialogTitle));
        content.AddChild(ViewFactory.CreateBoundUntranslatedLabel(
            nameof(MatchSearchViewModel.PresenceMessage),
            TextStyles.DialogMessage,
            DialogMetrics.ContentWidth));
        content.AddChild(confirmButton);
        return content;
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
