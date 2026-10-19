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
    private const float HaloSpread = 4;
    private const float PaddingX = 14;

    private readonly RectangleRuntime _halo;
    private readonly RectangleRuntime _surface;
    private InteractionFlags _flags;
    private bool _hasError;

    private FieldSkin(TextBoxBase field)
    {
        _halo = Shapes.CreateHalo(Radius, HaloSpread);
        _surface = Shapes.CreateRounded(Palette.White, Radius);
        _surface.StrokeWidth = BorderWidth;

        Shapes.HideDefaultParts(field);
        Shapes.InsertBehindContent(field.Visual, _halo, _surface);
        StyleText((TextBoxBaseVisual)field.Visual);
    }

    public bool HasError
    {
        get => _hasError;
        set
        {
            _hasError = value;
            Show(_flags);
        }
    }

    public static FieldSkin Apply(TextBoxBase field)
    {
        ArgumentNullException.ThrowIfNull(field);

        var skin = new FieldSkin(field);
        skin.Attach(field);
        return skin;
    }

    public override void Show(InteractionFlags flags)
    {
        _flags = flags;
        bool isFocused = flags.HasFlag(InteractionFlags.Focused);

        _surface.FillColor = flags.HasFlag(InteractionFlags.Disabled) ? Palette.Disabled : Palette.White;
        _surface.StrokeColor = PickBorder(flags);
        _halo.FillColor = _hasError ? Palette.ErrorHalo : Palette.FocusHalo;
        _halo.Visible = _hasError || isFocused;
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

        return flags.HasFlag(InteractionFlags.Hovered) ? Palette.BorderHover : Palette.Border;
    }

    private static void StyleText(TextBoxBaseVisual visual)
    {
        visual.ClipContainer.Width = -PaddingX * 2;
        visual.TextInstance.X = 0;
        visual.TextInstance.LocalizeText = false;
        visual.PlaceholderTextInstance.X = 0;
        TextStyles.FieldText.ApplyTo(visual.TextInstance);
        TextStyles.Placeholder.ApplyTo(visual.PlaceholderTextInstance);
        visual.ForegroundColor = TextStyles.FieldText.Color;
        visual.PlaceholderColor = TextStyles.Placeholder.Color;
        visual.CaretColor = Palette.Accent;
    }
}
