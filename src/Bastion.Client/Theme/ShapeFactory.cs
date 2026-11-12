using System;
using Gum.Forms.Controls;
using Gum.GueDeriving;
using Gum.Wireframe;
using Microsoft.Xna.Framework;

namespace Bastion.Client.Theme;

public static class ShapeFactory
{
    public const float NoStrokeWidth = 0;
    public const float FullPercentage = 100;

    private const string BackgroundPartName = "Background";
    private const string FocusedIndicatorPartName = "FocusedIndicator";
    private const int RadiiPerDiameter = 2;
    private const int SidesPerAxis = 2;
    private const float HaloSpread = 4;

    public static RectangleRuntime CreateRounded(Color fill, float radius)
    {
        var shape = new RectangleRuntime
        {
            CornerRadius = radius,
            IsFilled = true,
            FillColor = fill,
            StrokeWidth = NoStrokeWidth,
        };
        shape.Dock(Dock.Fill);
        return shape;
    }

    public static RectangleRuntime CreateCircle(Color fill, float diameter)
    {
        float radius = diameter / RadiiPerDiameter;
        return CreateRounded(fill, radius);
    }

    public static RectangleRuntime CreateHalo(float radius)
    {
        RectangleRuntime halo = CreateRounded(Palette.FocusHalo, radius + HaloSpread);
        float haloGrowth = HaloSpread * SidesPerAxis;
        halo.Width = haloGrowth;
        halo.Height = haloGrowth;
        halo.Visible = false;
        return halo;
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
        if (part is not null)
        {
            part.Visible = false;
        }
    }
}
