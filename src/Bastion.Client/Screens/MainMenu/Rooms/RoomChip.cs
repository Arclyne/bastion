using System;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.GueDeriving;
using Microsoft.Xna.Framework;

namespace Bastion.Client.Screens.MainMenu.Rooms;

public static class RoomChip
{
    private const float ChipHeight = 26;
    private const float PaddingX = 10;
    private const float ChipRadius = 13;

    public static Panel Create(string textKey, TextStyle style, Color fill)
    {
        ArgumentNullException.ThrowIfNull(textKey);
        ArgumentNullException.ThrowIfNull(style);

        var chipPanel = new Panel
        {
            Width = PaddingX,
            WidthUnits = DimensionUnitType.RelativeToChildren,
            Height = ChipHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        RectangleRuntime background = ShapeFactory.CreateRounded(fill, ChipRadius);
        ShapeFactory.InsertBehindContent(chipPanel.Visual, background);
        Label textLabel = ViewFactory.CreateLabel(textKey, style);
        ViewFactory.CenterVertically(textLabel);
        textLabel.X = PaddingX;
        chipPanel.AddChild(textLabel);
        return chipPanel;
    }
}
