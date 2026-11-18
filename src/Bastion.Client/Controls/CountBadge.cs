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
        Label countLabel = CreateCount(countProperty);
        ShapeFactory.InsertBehindContent(Visual, circle);
        AddChild(countLabel);
    }

    private static Label CreateCount(string countProperty)
    {
        Label countLabel = ViewFactory.CreateBoundUntranslatedLabel(countProperty, TextStyleCatalog.PrimaryButton);
        ViewFactory.CenterHorizontally(countLabel);
        ViewFactory.CenterVertically(countLabel);
        return countLabel;
    }
}
