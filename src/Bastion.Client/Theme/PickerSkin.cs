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
    private const float MiddleOffsetY = 0;
    private const int RadiiPerHeight = 2;

    private readonly RectangleRuntime _halo;
    private readonly RectangleRuntime _surface;

    private PickerSkin(ComboBox picker)
    {
        float radius = picker.Height / RadiiPerHeight;
        _halo = ShapeFactory.CreateHalo(radius);
        _surface = ShapeFactory.CreateRounded(Palette.Card, radius);
        _surface.StrokeWidth = BorderWidth;

        ShapeFactory.HideDefaultParts(picker);
        ShapeFactory.InsertBehindContent(picker.Visual, _halo, _surface);

        var visual = (ComboBoxVisual)picker.Visual;
        TextStyleCatalog.Picker.ApplyTo(visual.TextInstance);
        visual.TextInstance.LocalizeText = false;
        visual.TextInstance.X = PaddingX;
        visual.ForegroundColor = TextStyleCatalog.Picker.Color;
        visual.DropdownIndicatorColor = TextStyleCatalog.Picker.Color;
        visual.DropdownIndicator.YUnits = GeneralUnitType.PixelsFromMiddle;
        visual.DropdownIndicator.Y = MiddleOffsetY;

        StyleList(picker.ListBox);
    }

    public static PickerSkin CreateAndAttach(ComboBox picker)
    {
        ArgumentNullException.ThrowIfNull(picker);

        var skin = new PickerSkin(picker);
        skin.Attach(picker);
        return skin;
    }

    public override void ShowState(InteractionFlags flags)
    {
        bool isActive = flags.HasFlag(InteractionFlags.Hovered) || flags.HasFlag(InteractionFlags.Pressed);

        _surface.FillColor = flags.HasFlag(InteractionFlags.Disabled) ? Palette.Disabled : Palette.Card;
        _surface.StrokeColor = isActive ? Palette.BorderHover : Palette.Border;
        _halo.Visible = flags.HasFlag(InteractionFlags.Focused);
    }

    private static void StyleList(ListBox list)
    {
        list.VerticalSizeMode = SizeMode.Auto;
        list.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
        list.ItemsCollectionChanged += HandleListItemsChanged;

        RectangleRuntime surface = ShapeFactory.CreateRounded(Palette.Card, ListRadius);
        surface.StrokeColor = Palette.Border;
        surface.StrokeWidth = BorderWidth;
        surface.HasDropshadow = true;
        surface.DropshadowColor = Palette.Shadow;
        surface.DropshadowOffsetY = ListShadowOffsetY;
        surface.DropshadowBlur = ListShadowBlur;

        ShapeFactory.HideDefaultParts(list);
        ShapeFactory.InsertBehindContent(list.Visual, surface);
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

    private static void HandleListItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (sender is not ListBox list)
        {
            return;
        }

        foreach (ListBoxItem item in list.ListBoxItems)
        {
            var itemVisual = (ListBoxItemVisual)item.Visual;
            StyleListItem(itemVisual);
        }
    }
}
