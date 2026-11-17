using System;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
using Gum.GueDeriving;
using Microsoft.Xna.Framework;

namespace Bastion.Client.Theme;

public sealed record TextStyle
{
    private readonly string _fontFamily;
    private readonly int _fontSize;

    public TextStyle(string fontFamily, int fontSize, Color color)
    {
        ArgumentNullException.ThrowIfNull(fontFamily);

        _fontFamily = fontFamily;
        _fontSize = fontSize;
        Color = color;
    }

    public Color Color { get; }

    public void ApplyTo(TextRuntime text)
    {
        ArgumentNullException.ThrowIfNull(text);

        text.Font = _fontFamily;
        text.FontSize = _fontSize;
        text.Color = Color;
    }

    public void ApplyTo(Label label)
    {
        ArgumentNullException.ThrowIfNull(label);

        var labelVisual = (LabelVisual)label.Visual;
        ApplyTo(labelVisual);
    }

    public Label CreateFixedWidthLabel(float width)
    {
        var label = new Label
        {
            Width = width,
            WidthUnits = DimensionUnitType.Absolute,
        };
        ApplyTo(label);
        return label;
    }
}
