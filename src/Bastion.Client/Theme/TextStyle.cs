using System;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
using Gum.GueDeriving;
using Microsoft.Xna.Framework;

namespace Bastion.Client.Theme;

public sealed record TextStyle
{
    public TextStyle(string fontFamily, int fontSize, Color color)
    {
        ArgumentNullException.ThrowIfNull(fontFamily);

        FontFamily = fontFamily;
        FontSize = fontSize;
        Color = color;
    }

    public string FontFamily { get; }

    public int FontSize { get; }

    public Color Color { get; }

    public void ApplyTo(TextRuntime text)
    {
        ArgumentNullException.ThrowIfNull(text);

        text.Font = FontFamily;
        text.FontSize = FontSize;
        text.Color = Color;
    }

    public void ApplyTo(Label label)
    {
        ArgumentNullException.ThrowIfNull(label);

        var labelVisual = (LabelVisual)label.Visual;
        ApplyTo(labelVisual);
    }
}
