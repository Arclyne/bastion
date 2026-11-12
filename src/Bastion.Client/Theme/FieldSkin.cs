using System;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
using Gum.GueDeriving;
using Microsoft.Xna.Framework;

namespace Bastion.Client.Theme;

public sealed class FieldSkin : InteractiveSkin
{
    private const float Radius = 13;
    private const float BorderWidth = 1.8f;
    private const float PaddingX = 14;
    private const float TextOffsetX = 0;
    private const int HorizontalSideCount = 2;

    private readonly RectangleRuntime _halo;
    private readonly RectangleRuntime _surface;
    private InteractionFlags _flags;
    private bool _hasError;

    private FieldSkin(TextBoxBase field)
    {
        _halo = ShapeFactory.CreateHalo(Radius);
        _surface = ShapeFactory.CreateRounded(Palette.White, Radius);
        _surface.StrokeWidth = BorderWidth;

        ShapeFactory.HideDefaultParts(field);
        ShapeFactory.InsertBehindContent(field.Visual, _halo, _surface);
        var fieldVisual = (TextBoxBaseVisual)field.Visual;
        StyleText(fieldVisual);
    }

    public bool HasError
    {
        get => _hasError;
        set
        {
            _hasError = value;
            ShowState(_flags);
        }
    }

    public static FieldSkin CreateAndAttach(TextBoxBase field)
    {
        ArgumentNullException.ThrowIfNull(field);

        var skin = new FieldSkin(field);
        skin.Attach(field);
        return skin;
    }

    public override void ShowState(InteractionFlags flags)
    {
        _flags = flags;
        bool isFocused = flags.HasFlag(InteractionFlags.Focused);

        _surface.FillColor = flags.HasFlag(InteractionFlags.Disabled) ? Palette.Disabled : Palette.White;
        _surface.StrokeColor = PickBorder(flags);
        _halo.FillColor = _hasError ? Palette.ErrorHalo : Palette.FocusHalo;
        _halo.Visible = _hasError || isFocused;
    }

    private static void StyleText(TextBoxBaseVisual visual)
    {
        visual.ClipContainer.Width = -(PaddingX * HorizontalSideCount);
        visual.TextInstance.X = TextOffsetX;
        visual.TextInstance.LocalizeText = false;
        visual.PlaceholderTextInstance.X = TextOffsetX;
        TextStyleCatalog.FieldText.ApplyTo(visual.TextInstance);
        TextStyleCatalog.Placeholder.ApplyTo(visual.PlaceholderTextInstance);
        visual.ForegroundColor = TextStyleCatalog.FieldText.Color;
        visual.PlaceholderColor = TextStyleCatalog.Placeholder.Color;
        visual.CaretColor = Palette.Accent;
    }

    private Color PickBorder(InteractionFlags flags)
    {
        if (_hasError)
        {
            return Palette.Error;
        }

        if (flags.HasFlag(InteractionFlags.Focused))
        {
            return Palette.Accent;
        }

        if (flags.HasFlag(InteractionFlags.Hovered))
        {
            return Palette.BorderHover;
        }

        return Palette.Border;
    }
}
