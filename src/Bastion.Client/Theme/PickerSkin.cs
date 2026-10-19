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
    private const float HaloSpread = 4;
    private const float PaddingX = 12;
    private const float ListRadius = 12;
    private const float ListItemHeight = 36;
    private const float ListShadowOffsetY = 6;
    private const float ListShadowBlur = 16;

    private readonly RectangleRuntime _halo;
    private readonly RectangleRuntime _surface;

    private PickerSkin(ComboBox picker)
    {
        float radius = picker.Height / 2;
        _halo = Shapes.CreateHalo(radius, HaloSpread);
        _surface = Shapes.CreateRounded(Palette.Card, radius);
        _surface.StrokeWidth = BorderWidth;

        Shapes.HideDefaultParts(picker);
        Shapes.InsertBehindContent(picker.Visual, _halo, _surface);

        var visual = (ComboBoxVisual)picker.Visual;
        TextStyles.Picker.ApplyTo(visual.TextInstance);
        visual.TextInstance.LocalizeText = false;
        visual.TextInstance.X = PaddingX;
        visual.ForegroundColor = TextStyles.Picker.Color;
        visual.DropdownIndicatorColor = TextStyles.Picker.Color;
        visual.DropdownIndicator.YUnits = GeneralUnitType.PixelsFromMiddle;
        visual.DropdownIndicator.Y = 0;

        StyleList(picker.ListBox);
    }

    public static PickerSkin Apply(ComboBox picker)
    {
        ArgumentNullException.ThrowIfNull(picker);

        var skin = new PickerSkin(picker);
        skin.Attach(picker);
        return skin;
    }

    public override void Show(InteractionFlags flags)
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
        list.ItemsCollectionChanged += OnListItemsChanged;

        RectangleRuntime surface = Shapes.CreateRounded(Palette.Card, ListRadius);
        surface.StrokeColor = Palette.Border;
        surface.StrokeWidth = BorderWidth;
        surface.HasDropshadow = true;
        surface.DropshadowColor = Palette.Shadow;
        surface.DropshadowOffsetY = ListShadowOffsetY;
        surface.DropshadowBlur = ListShadowBlur;

        Shapes.HideDefaultParts(list);
        Shapes.InsertBehindContent(list.Visual, surface);
    }

    private static void OnListItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (sender is not ListBox list)
        {
            return;
        }

        foreach (ListBoxItem item in list.ListBoxItems)
        {
            StyleListItem((ListBoxItemVisual)item.Visual);
        }
    }

    private static void StyleListItem(ListBoxItemVisual item)
    {
        TextStyles.Picker.ApplyTo(item.TextInstance);
        item.TextInstance.LocalizeText = false;
        item.TextInstance.X = PaddingX;
        item.ForegroundColor = TextStyles.Picker.Color;
        item.HighlightedBackgroundColor = Palette.SurfaceHover;
        item.SelectedBackgroundColor = Palette.Selected;
        item.Height = ListItemHeight;
        item.HeightUnits = DimensionUnitType.Absolute;
    }
}
