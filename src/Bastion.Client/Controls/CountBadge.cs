using System;
using Bastion.Client.Screens;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.GueDeriving;

namespace Bastion.Client.Controls;

public sealed class CountBadge : Panel
{
    private const float Size = 22;

    public CountBadge(string countProperty)
    {
        ArgumentNullException.ThrowIfNull(countProperty);

        Width = Size;
        WidthUnits = DimensionUnitType.Absolute;
        Height = Size;
        HeightUnits = DimensionUnitType.Absolute;
        RectangleRuntime circle = ShapeFactory.CreateCircle(Palette.IdentityAccent, Size);
        Label count = CreateCount(countProperty);
        ShapeFactory.InsertBehindContent(Visual, circle);
        AddChild(count);
    }

    private static Label CreateCount(string countProperty)
    {
        Label count = ViewFactory.CreateBoundUntranslatedLabel(countProperty, TextStyleCatalog.PrimaryButton);
        ViewFactory.CenterHorizontally(count);
        ViewFactory.CenterVertically(count);
        return count;
    }
}
