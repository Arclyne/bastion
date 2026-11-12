using System;
using Gum.Converters;
using Gum.GueDeriving;
using Gum.Wireframe;
using Microsoft.Xna.Framework;

namespace Bastion.Client.Theme;

public static class CardSkin
{
    private const float CardRadius = 26;
    private const float CardBorderWidth = 1.5f;
    private const float CardStripeHeight = 8;
    private const float CardShadowOffsetY = 30;
    private const float CardShadowBlur = 40;
    private const int VerticalSideCount = 2;

    public static void Apply(GraphicalUiElement card)
    {
        ArgumentNullException.ThrowIfNull(card);

        RectangleRuntime stripe = ShapeFactory.CreateRounded(Palette.Accent, CardRadius);
        stripe.UseGradient = true;
        stripe.Color2 = Palette.AccentSoft;
        stripe.GradientX2 = ShapeFactory.FullPercentage;
        stripe.GradientX2Units = GeneralUnitType.Percentage;
        stripe.HasDropshadow = true;
        stripe.DropshadowColor = Palette.Shadow;
        stripe.DropshadowOffsetY = CardShadowOffsetY;
        stripe.DropshadowBlur = CardShadowBlur;

        RectangleRuntime surface = ShapeFactory.CreateRounded(Palette.Card, CardRadius);
        surface.Y = CardStripeHeight / VerticalSideCount;
        surface.Height = -CardStripeHeight;

        RectangleRuntime border = ShapeFactory.CreateRounded(Color.Transparent, CardRadius);
        border.StrokeColor = Palette.BorderStrong;
        border.StrokeWidth = CardBorderWidth;

        ShapeFactory.InsertBehindContent(card, stripe, surface, border);
    }
}
