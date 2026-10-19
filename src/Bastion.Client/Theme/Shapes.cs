using Gum.Forms.Controls;
using Gum.GueDeriving;
using Gum.Wireframe;
using Microsoft.Xna.Framework;

namespace Bastion.Client.Theme;

public static class Shapes
{
    private const string BackgroundPartName = "Background";
    private const string FocusedIndicatorPartName = "FocusedIndicator";

    public static RectangleRuntime CreateRounded(Color fill, float radius)
    {
        var shape = new RectangleRuntime
        {
            CornerRadius = radius,
            IsFilled = true,
            FillColor = fill,
            StrokeWidth = 0,
        };
        shape.Dock(Dock.Fill);
        return shape;
    }

    public static RectangleRuntime CreateHalo(float radius, float spread)
    {
        RectangleRuntime halo = CreateRounded(Palette.FocusHalo, radius + spread);
        halo.Width = spread * 2;
        halo.Height = spread * 2;
        halo.Visible = false;
        return halo;
    }

    public static void InsertBehindContent(GraphicalUiElement visual, params GraphicalUiElement[] layers)
    {
        for (int layerIndex = 0; layerIndex < layers.Length; layerIndex++)
        {
            visual.Children.Insert(layerIndex, layers[layerIndex]);
        }
    }

    public static void HideDefaultParts(FrameworkElement control)
    {
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
