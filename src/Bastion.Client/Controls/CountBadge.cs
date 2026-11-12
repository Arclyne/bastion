using System;
using Bastion.Client.Screens;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;

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
        Shapes.InsertBehindContent(Visual, Shapes.CreateCircle(Palette.PlayerOne, Size));
        AddChild(CreateCount(countProperty));
    }

    private static Label CreateCount(string countProperty)
    {
        Label count = ViewFactory.CreateBoundUntranslatedLabel(countProperty, TextStyles.PrimaryButton);
        ViewFactory.CenterHorizontally(count);
        ViewFactory.CenterVertically(count);
        return count;
    }
}
