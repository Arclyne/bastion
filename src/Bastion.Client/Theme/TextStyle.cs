using System;
using Gum.GueDeriving;
using Microsoft.Xna.Framework;

namespace Bastion.Client.Theme;

public sealed record TextStyle(string FontFamily, int FontSize, Color Color)
{
    public void ApplyTo(TextRuntime text)
    {
        ArgumentNullException.ThrowIfNull(text);

        text.Font = FontFamily;
        text.FontSize = FontSize;
        text.Color = Color;
    }
}
