using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.GueDeriving;
using Microsoft.Xna.Framework;

namespace Bastion.Client.Screens.MainMenu.Rooms;

public sealed class RoomNoticeStrip : Panel
{
    private const float StripWidth = 560;
    private const float StripY = 30;
    private const float PaddingX = 14;
    private const float PaddingY = 10;
    private const float StripRadius = 16;
    private const float CloseSize = 32;
    private const float CloseGap = 8;
    private const int SideCount = 2;
    private const float TextWidth = StripWidth - (PaddingX * SideCount) - CloseSize - CloseGap;

    public RoomNoticeStrip(Color fill, NoticeBinding binding, EventHandler handleClose)
    {
        ArgumentNullException.ThrowIfNull(binding);
        ArgumentNullException.ThrowIfNull(handleClose);

        Width = StripWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Height = PaddingY * SideCount;
        HeightUnits = DimensionUnitType.RelativeToChildren;
        Anchor(Gum.Wireframe.Anchor.Top);
        Y = StripY;
        SetBinding(nameof(IsVisible), binding.VisibilityProperty);
        RectangleRuntime background = ShapeFactory.CreateRounded(fill, StripRadius);
        ShapeFactory.InsertBehindContent(Visual, background);
        Label messageLabel = ViewFactory.CreateBoundUntranslatedLabel(
            binding.TextProperty,
            TextStyleCatalog.NoticeMessage,
            TextWidth);
        messageLabel.X = PaddingX;
        messageLabel.Y = PaddingY;
        Button closeButton = ViewFactory.CreateButton(nameof(Strings.RoomDismiss), CloseSize, CloseSize);
        ButtonSkin.CreateAndAttach(closeButton, ButtonLookCatalog.Pill);
        closeButton.Anchor(Gum.Wireframe.Anchor.Right);
        closeButton.X = -PaddingX;
        closeButton.Click += handleClose;
        AddChild(messageLabel);
        AddChild(closeButton);
    }
}
