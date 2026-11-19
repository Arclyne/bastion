using System;
using System.Collections.Generic;
using Bastion.Client.Theme;
using Gum.Converters;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.GueDeriving;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RenderingLibrary.Graphics;

namespace Bastion.Client.Screens;

public sealed class ConnectionBadge : Panel
{
    private const float BadgeWidth = 320;
    private const float PaddingTop = 26;
    private const float PaddingBottom = 24;
    private const float Spacing = 12;
    private const float IconDiscSize = 76;
    private const float IconSize = 44;

    public ConnectionBadge(Texture2D icon, Color iconFill, IReadOnlyList<Label> lines)
    {
        ArgumentNullException.ThrowIfNull(icon);
        ArgumentNullException.ThrowIfNull(lines);

        Width = BadgeWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Height = PaddingBottom;
        HeightUnits = DimensionUnitType.RelativeToChildren;
        var contentStackPanel = new StackPanel
        {
            Width = BadgeWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Y = PaddingTop,
            Spacing = Spacing,
        };
        Panel iconPanel = CreateIcon(icon, iconFill);
        ViewFactory.CenterHorizontally(iconPanel);
        contentStackPanel.AddChild(iconPanel);
        foreach (Label line in lines)
        {
            ViewFactory.CenterHorizontally(line);
            contentStackPanel.AddChild(line);
        }

        AddChild(contentStackPanel);
        CardSkin.Apply(Visual);
    }

    private static Panel CreateIcon(Texture2D icon, Color iconFill)
    {
        var discPanel = new Panel
        {
            Width = IconDiscSize,
            WidthUnits = DimensionUnitType.Absolute,
            Height = IconDiscSize,
            HeightUnits = DimensionUnitType.Absolute,
        };
        RectangleRuntime disc = ShapeFactory.CreateCircle(iconFill, IconDiscSize);
        ShapeFactory.InsertBehindContent(discPanel.Visual, disc);
        var sprite = new SpriteRuntime
        {
            Texture = icon,
            Width = IconSize,
            WidthUnits = DimensionUnitType.Absolute,
            Height = IconSize,
            HeightUnits = DimensionUnitType.Absolute,
            XOrigin = HorizontalAlignment.Center,
            XUnits = GeneralUnitType.PixelsFromMiddle,
            YOrigin = VerticalAlignment.Center,
            YUnits = GeneralUnitType.PixelsFromMiddle,
        };
        discPanel.Visual.Children.Add(sprite);
        return discPanel;
    }
}
