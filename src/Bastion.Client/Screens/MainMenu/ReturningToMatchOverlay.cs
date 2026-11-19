using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.Forms.Controls;
using Gum.GueDeriving;

namespace Bastion.Client.Screens.MainMenu;

public sealed class ReturningToMatchOverlay : Panel
{
    public ReturningToMatchOverlay(MainMenuViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        BindingContext = viewModel;
        this.Dock(Gum.Wireframe.Dock.Fill);
        SetBinding(nameof(IsVisible), nameof(MainMenuViewModel.IsReturningToMatch));
        RectangleRuntime veil = ShapeFactory.CreateRounded(Palette.Overlay, radius: 0);
        ShapeFactory.InsertBehindContent(Visual, veil);
        Label detailLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(MainMenuViewModel.ReturningDetailText),
            TextStyleCatalog.BadgeDetail);
        Label[] lines =
        [
            ViewFactory.CreateLabel(nameof(Strings.ReturningToMatchTitle), TextStyleCatalog.BadgeTitle),
            detailLabel,
            ViewFactory.CreateLabel(nameof(Strings.LostConnectionClock), TextStyleCatalog.BadgeNote),
        ];
        var badge = new ConnectionBadge(IconCatalog.Rejoin, Palette.RejoinSoft, lines);
        badge.Anchor(Gum.Wireframe.Anchor.Center);
        AddChild(badge);
    }
}
