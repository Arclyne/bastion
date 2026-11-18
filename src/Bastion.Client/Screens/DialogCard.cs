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

        var contentStackPanel = new StackPanel
        {
            X = DialogMetrics.PaddingX,
            Y = DialogMetrics.PaddingTop,
            Spacing = DialogMetrics.SectionSpacing,
        };
        AddChild(contentStackPanel);
        Content = contentStackPanel;
        CardSkin.Apply(Visual);
    }

    protected StackPanel Content { get; }

    protected static Button CreateDialogButton(string textKey, ButtonLook buttonLook)
    {
        ArgumentNullException.ThrowIfNull(textKey);
        ArgumentNullException.ThrowIfNull(buttonLook);

        Button button = ViewFactory.CreateButton(textKey, DialogMetrics.ButtonWidth, DialogMetrics.ButtonHeight);
        ButtonSkin.CreateAndAttach(button, buttonLook);
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
