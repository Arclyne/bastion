using System;
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
        CardSkin.Apply(Visual);
    }

    protected StackPanel Content { get; }

    protected static Button CreateDialogButton(string textKey, ButtonLook look)
    {
        ArgumentNullException.ThrowIfNull(textKey);
        ArgumentNullException.ThrowIfNull(look);

        Button button = ViewFactory.CreateButton(textKey, DialogMetrics.ButtonWidth, DialogMetrics.ButtonHeight);
        ButtonSkin.CreateAndAttach(button, look);
        return button;
    }

    protected static Button CreateWideButton(string textKey, EventHandler handleClick)
    {
        ArgumentNullException.ThrowIfNull(textKey);
        ArgumentNullException.ThrowIfNull(handleClick);

        Button button = ViewFactory.CreateButton(textKey, DialogMetrics.ContentWidth, DialogMetrics.ButtonHeight);
        ButtonSkin.CreateAndAttach(button, ButtonLookCatalog.Primary);
        button.Click += handleClick;
        return button;
    }
}
