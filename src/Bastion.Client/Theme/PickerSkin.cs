using System;
using System.Collections.Specialized;
using Gum.Converters;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
using Gum.GueDeriving;

namespace Bastion.Client.Theme;

public sealed class PickerSkin : InteractiveSkin
{
    private const float BorderWidth = 1.5f;
    private const float PaddingX = 12;
    private const float ListRadius = 12;
    private const float ListItemHeight = 36;
    private const float ListShadowOffsetY = 6;
    private const float ListShadowBlur = 16;
    private const int RadiiPerHeight = 2;

    private readonly RectangleRuntime _halo;
    private readonly RectangleRuntime _surface;

    private PickerSkin(ComboBox pickerComboBox)
    {
        float radius = pickerComboBox.Height / RadiiPerHeight;
        _halo = ShapeFactory.CreateHalo(radius);
        _surface = ShapeFactory.CreateRounded(Palette.Card, radius);
        _surface.StrokeWidth = BorderWidth;

        ShapeFactory.HideDefaultParts(pickerComboBox);
        ShapeFactory.InsertBehindContent(pickerComboBox.Visual, _halo, _surface);

        var visual = (ComboBoxVisual)pickerComboBox.Visual;
        TextStyleCatalog.Picker.ApplyTo(visual.TextInstance);
        visual.TextInstance.LocalizeText = false;
        visual.TextInstance.X = PaddingX;
        visual.ForegroundColor = TextStyleCatalog.Picker.Color;
        visual.DropdownIndicatorColor = TextStyleCatalog.Picker.Color;
        visual.DropdownIndicator.YUnits = GeneralUnitType.PixelsFromMiddle;
        visual.DropdownIndicator.Y = 0;

        StyleList(pickerComboBox.ListBox);
    }

    public static PickerSkin CreateAndAttach(ComboBox pickerComboBox)
    {
        ArgumentNullException.ThrowIfNull(pickerComboBox);

        var skin = new PickerSkin(pickerComboBox);
        skin.Attach(pickerComboBox);
        return skin;
    }

    public override void ShowState(InteractionStates flags)
    {
        bool isActive = flags.HasFlag(InteractionStates.Hovered) || flags.HasFlag(InteractionStates.Pressed);

        _surface.FillColor = flags.HasFlag(InteractionStates.Disabled) ? Palette.Disabled : Palette.Card;
        _surface.StrokeColor = isActive ? Palette.BorderHover : Palette.Border;
        _halo.Visible = flags.HasFlag(InteractionStates.Focused);
    }

    private static void StyleList(ListBox listBox)
    {
        listBox.VerticalSizeMode = SizeMode.Auto;
        listBox.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
        listBox.ItemsCollectionChanged += HandleListItemsChanged;

        RectangleRuntime surface = ShapeFactory.CreateRaisedSurface(ListRadius, ListShadowOffsetY, ListShadowBlur);

        ShapeFactory.HideDefaultParts(listBox);
        ShapeFactory.InsertBehindContent(listBox.Visual, surface);
    }

    private static void HandleListItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (sender is not ListBox listBox)
        {
            return;
        }

        foreach (ListBoxItem listBoxItem in listBox.ListBoxItems)
        {
            var itemVisual = (ListBoxItemVisual)listBoxItem.Visual;
            StyleListItem(itemVisual);
        }
    }

    private static void StyleListItem(ListBoxItemVisual item)
    {
        TextStyleCatalog.Picker.ApplyTo(item.TextInstance);
        DisableTranslationKeepingText(item.TextInstance);
        item.TextInstance.X = PaddingX;
        item.ForegroundColor = TextStyleCatalog.Picker.Color;
        item.HighlightedBackgroundColor = Palette.SurfaceHover;
        item.SelectedBackgroundColor = Palette.Selected;
        item.Height = ListItemHeight;
        item.HeightUnits = DimensionUnitType.Absolute;
    }

    private static void DisableTranslationKeepingText(TextRuntime text)
    {
        string? shownText = text.Text;
        text.LocalizeText = false;
        text.Text = shownText;
    }
}
