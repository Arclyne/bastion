using System;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.GueDeriving;
using Gum.Wireframe;
using Microsoft.Xna.Framework;

namespace Bastion.Client.Theme;

public static class ShapeFactory
{
    public const float FullPercentage = 100;

    private const string BackgroundPartName = "Background";
    private const string FocusedIndicatorPartName = "FocusedIndicator";
    private const int RadiiPerDiameter = 2;
    private const int SidesPerAxis = 2;
    private const float HaloSpread = 4;
    private const float HaloGrowth = HaloSpread * SidesPerAxis;
    private const float RaisedBorderWidth = 1.5f;

    public static RectangleRuntime CreateRounded(Color fillColor, float radius)
    {
        var shape = new RectangleRuntime
        {
            CornerRadius = radius,
            IsFilled = true,
            FillColor = fillColor,
            StrokeWidth = 0,
        };
        shape.Dock(Dock.Fill);
        return shape;
    }

    public static RectangleRuntime CreateCircle(Color fillColor, float diameter)
    {
        float radius = diameter / RadiiPerDiameter;
        return CreateRounded(fillColor, radius);
    }

    public static RectangleRuntime CreateHalo(float radius)
    {
        RectangleRuntime halo = CreateRounded(Palette.FocusHalo, radius + HaloSpread);
        halo.Width = HaloGrowth;
        halo.Height = HaloGrowth;
        halo.Visible = false;
        return halo;
    }

    public static RectangleRuntime CreateRaisedSurface(float radius, float shadowOffsetY, float shadowBlur)
    {
        RectangleRuntime surface = CreateRounded(Palette.Card, radius);
        surface.StrokeColor = Palette.Border;
        surface.StrokeWidth = RaisedBorderWidth;
        surface.HasDropshadow = true;
        surface.DropshadowColor = Palette.Shadow;
        surface.DropshadowOffsetY = shadowOffsetY;
        surface.DropshadowBlur = shadowBlur;
        return surface;
    }

    public static Panel CreateAvatar(float size, Color ringColor, float ringWidth)
    {
        var avatarPanel = new Panel
        {
            Width = size,
            WidthUnits = DimensionUnitType.Absolute,
            Height = size,
            HeightUnits = DimensionUnitType.Absolute,
        };
        RectangleRuntime circle = CreateCircle(Palette.AvatarFill, size);
        circle.StrokeColor = ringColor;
        circle.StrokeWidth = ringWidth;
        InsertBehindContent(avatarPanel.Visual, circle);
        return avatarPanel;
    }

    public static void InsertBehindContent(GraphicalUiElement visual, params GraphicalUiElement[] layers)
    {
        ArgumentNullException.ThrowIfNull(visual);
        ArgumentNullException.ThrowIfNull(layers);

        for (int layerIndex = 0; layerIndex < layers.Length; layerIndex++)
        {
            visual.Children.Insert(layerIndex, layers[layerIndex]);
        }
    }

    public static void HideDefaultParts(FrameworkElement control)
    {
        ArgumentNullException.ThrowIfNull(control);

        HidePart(control, BackgroundPartName);
        HidePart(control, FocusedIndicatorPartName);
    }

    private static void HidePart(FrameworkElement control, string partName)
    {
        GraphicalUiElement? part = control.GetVisual(partName);
        if (part is null)
        {
            return;
        }

        part.Visible = false;
    }
}
