using System;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.GueDeriving;

namespace Bastion.Client.Controls;

public sealed class TabButton : StackPanel
{
    private const float MarkerHeight = 3;
    private const float MarkerRadius = MarkerHeight / RadiiPerHeight;
    private const int RadiiPerHeight = 2;

    public TabButton(Button button, string selectedProperty, float spacing)
    {
        ArgumentNullException.ThrowIfNull(button);
        ArgumentNullException.ThrowIfNull(selectedProperty);

        Spacing = spacing;
        Panel marker = CreateMarker(button.Width, selectedProperty);
        AddChild(button);
        AddChild(marker);
    }

    private static Panel CreateMarker(float width, string selectedProperty)
    {
        RectangleRuntime marker = ShapeFactory.CreateRounded(Palette.Accent, MarkerRadius);
        var markerHolder = new Panel
        {
            Width = width,
            WidthUnits = DimensionUnitType.Absolute,
            Height = MarkerHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        markerHolder.AddChild(marker);
        markerHolder.SetBinding(nameof(IsVisible), selectedProperty);
        return markerHolder;
    }
}
