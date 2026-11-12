using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;

namespace Bastion.Client.Screens;

public abstract class DialogCard : Panel
{
    protected DialogCard()
    {
        Width = DialogMetrics.Width;
        WidthUnits = DimensionUnitType.Absolute;
        Height = DialogMetrics.PaddingBottom;
        HeightUnits = DimensionUnitType.RelativeToChildren;

        var content = new StackPanel
        {
            X = DialogMetrics.PaddingX,
            Y = DialogMetrics.PaddingTop,
            Spacing = DialogMetrics.SectionSpacing,
        };
        AddChild(content);
        Content = content;
        Skins.ApplyCard(Visual);
    }

    protected StackPanel Content { get; }
}
