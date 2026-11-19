using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.Forms.Controls;
using Gum.GueDeriving;

namespace Bastion.Client.Screens.Matches;

public sealed class LostConnectionOverlay : Panel
{
    public LostConnectionOverlay(MatchConnectionViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        BindingContext = viewModel;
        this.Dock(Gum.Wireframe.Dock.Fill);
        SetBinding(nameof(IsVisible), nameof(MatchConnectionViewModel.IsReconnecting));
        RectangleRuntime veil = ShapeFactory.CreateRounded(Palette.Overlay, radius: 0);
        ShapeFactory.InsertBehindContent(Visual, veil);
        Label[] lines =
        [
            ViewFactory.CreateLabel(nameof(Strings.LostConnectionTitle), TextStyleCatalog.BadgeTitle),
            ViewFactory.CreateLabel(nameof(Strings.LostConnectionDetail), TextStyleCatalog.BadgeDetail),
            ViewFactory.CreateLabel(nameof(Strings.LostConnectionClock), TextStyleCatalog.BadgeNote),
        ];
        var badge = new ConnectionBadge(IconCatalog.LostConnection, Palette.ErrorSoft, lines);
        badge.Anchor(Gum.Wireframe.Anchor.Center);
        AddChild(badge);
    }
}
