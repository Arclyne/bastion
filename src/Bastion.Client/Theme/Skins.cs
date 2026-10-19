using System;
using Gum.Converters;
using Gum.GueDeriving;
using Gum.Wireframe;
using Microsoft.Xna.Framework;

namespace Bastion.Client.Theme;

public static class Skins
{
    private const float CardRadius = 26;
    private const float CardBorderWidth = 1.5f;
    private const float CardStripeHeight = 8;
    private const float CardShadowOffsetY = 30;
    private const float CardShadowBlur = 40;
    private const float FullPercentage = 100;

    public static void ApplyCard(GraphicalUiElement card)
    {
        ArgumentNullException.ThrowIfNull(card);

        RectangleRuntime stripe = Shapes.CreateRounded(Palette.Accent, CardRadius);
        stripe.UseGradient = true;
        stripe.Color2 = Palette.AccentSoft;
        stripe.GradientX2 = FullPercentage;
        stripe.GradientX2Units = GeneralUnitType.Percentage;
        stripe.HasDropshadow = true;
        stripe.DropshadowColor = Palette.Shadow;
        stripe.DropshadowOffsetY = CardShadowOffsetY;
        stripe.DropshadowBlur = CardShadowBlur;

        RectangleRuntime surface = Shapes.CreateRounded(Palette.Card, CardRadius);
        surface.Y = CardStripeHeight / 2;
        surface.Height = -CardStripeHeight;

        RectangleRuntime border = Shapes.CreateRounded(Color.Transparent, CardRadius);
        border.StrokeColor = Palette.BorderStrong;
        border.StrokeWidth = CardBorderWidth;

        Shapes.InsertBehindContent(card, stripe, surface, border);
    }
}
