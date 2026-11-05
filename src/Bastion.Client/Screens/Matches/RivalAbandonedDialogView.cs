using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;

namespace Bastion.Client.Screens.Matches;

public sealed class RivalAbandonedDialogView : Panel
{
    private readonly MatchAbandonmentViewModel _viewModel;

    public RivalAbandonedDialogView(MatchAbandonmentViewModel viewModel)
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
        Button menuButton = ViewFactory.CreateButton(
            nameof(Strings.MatchBackToMenuButton),
            DialogMetrics.ContentWidth,
            DialogMetrics.ButtonHeight);
        ButtonSkin.CreateAndAttach(menuButton, ButtonLooks.Primary);
        menuButton.Click += HandleMenuClick;

        var content = new StackPanel
        {
            X = DialogMetrics.PaddingX,
            Y = DialogMetrics.PaddingTop,
            Spacing = DialogMetrics.SectionSpacing,
        };
        content.AddChild(ViewFactory.CreateLabel(nameof(Strings.MatchOverTitle), TextStyles.DialogTitle));
        content.AddChild(ViewFactory.CreateBoundUntranslatedLabel(
            nameof(MatchAbandonmentViewModel.RivalAbandonedText),
            TextStyles.DialogMessage,
            DialogMetrics.ContentWidth));
        content.AddChild(menuButton);
        return content;
    }

    private void HandleMenuClick(object? sender, EventArgs e)
    {
        _viewModel.RequestReturnToMenu();
    }
}
