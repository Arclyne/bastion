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
        Panel markerPanel = CreateMarker(button.Width, selectedProperty);
        AddChild(button);
        AddChild(markerPanel);
    }

    private static Panel CreateMarker(float width, string selectedProperty)
    {
        RectangleRuntime marker = ShapeFactory.CreateRounded(Palette.Accent, MarkerRadius);
        var markerHolderPanel = new Panel
        {
            Width = width,
            WidthUnits = DimensionUnitType.Absolute,
            Height = MarkerHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        markerHolderPanel.AddChild(marker);
        markerHolderPanel.SetBinding(nameof(IsVisible), selectedProperty);
        return markerHolderPanel;
    }
}
