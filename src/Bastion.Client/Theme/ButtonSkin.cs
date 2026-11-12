using System;
using Gum.Converters;
using Gum.Forms.Controls.Primitives;
using Gum.Forms.DefaultVisuals.V3;
using Gum.GueDeriving;
using Microsoft.Xna.Framework;

namespace Bastion.Client.Theme;

public sealed class ButtonSkin : InteractiveSkin
{
    private const float BorderWidth = 1.8f;
    private const float RestingOffsetY = 0;

    private readonly ButtonLook _look;
    private readonly RectangleRuntime _halo;
    private readonly RectangleRuntime _surface;
    private readonly TextRuntime _text;

    private ButtonSkin(ButtonBase button, ButtonLook look)
    {
        _look = look;
        _halo = ShapeFactory.CreateHalo(look.Radius);
        _surface = ShapeFactory.CreateRounded(look.Fill, look.Radius);
        _surface.StrokeWidth = look.Border is null ? ShapeFactory.NoStrokeWidth : BorderWidth;
        _surface.GradientY2 = ShapeFactory.FullPercentage;
        _surface.GradientY2Units = GeneralUnitType.Percentage;
        _surface.HasDropshadow = true;
        _surface.DropshadowColor = look.ShadowColor;
        _surface.DropshadowBlur = look.ShadowBlur;
        _text = FindText(button);
        look.Text.ApplyTo(_text);

        ShapeFactory.HideDefaultParts(button);
        ShapeFactory.InsertBehindContent(button.Visual, _halo, _surface);
    }

    public static ButtonSkin CreateAndAttach(ButtonBase button, ButtonLook look)
    {
        ArgumentNullException.ThrowIfNull(button);
        ArgumentNullException.ThrowIfNull(look);

        var skin = new ButtonSkin(button, look);
        skin.Attach(button);
        return skin;
    }

    public override void ShowState(InteractionFlags flags)
    {
        bool isDisabled = flags.HasFlag(InteractionFlags.Disabled);
        bool isHovered = flags.HasFlag(InteractionFlags.Hovered);
        float sink = IsSinking(flags) ? _look.ShadowOffsetY : RestingOffsetY;

        _surface.FillColor = PickFill(flags);
        ApplyGradient(isHovered ? _look.HoverFillEnd : _look.FillEnd, isDisabled);
        _surface.StrokeColor = (isHovered ? _look.HoverBorder : _look.Border) ?? Color.Transparent;
        _surface.DropshadowOffsetY = _look.ShadowOffsetY - sink;
        _surface.Y = sink;
        _text.Y = sink;
        _text.Color = isDisabled ? Palette.Muted : _look.Text.Color;
        _halo.Y = sink;
        _halo.Visible = flags.HasFlag(InteractionFlags.Focused) && !isDisabled;
    }

    private static TextRuntime FindText(ButtonBase button)
    {
        switch (button.Visual)
        {
            case ButtonVisual buttonVisual:
                return buttonVisual.TextInstance;
            case ToggleButtonVisual toggleVisual:
                return toggleVisual.TextInstance;
            default:
                throw new ArgumentException("The button does not use a default Gum visual.", nameof(button));
        }
    }

    private bool IsSinking(InteractionFlags flags)
    {
        return _look.ShouldSinkWhenPressed && flags.HasFlag(InteractionFlags.Pressed);
    }

    private Color PickFill(InteractionFlags flags)
    {
        if (flags.HasFlag(InteractionFlags.Disabled))
        {
            return Palette.Disabled;
        }

        if (flags.HasFlag(InteractionFlags.Checked))
        {
            return _look.SelectedFill;
        }

        if (flags.HasFlag(InteractionFlags.Hovered))
        {
            return _look.HoverFill;
        }

        return _look.Fill;
    }

    private void ApplyGradient(Color? fillEnd, bool isDisabled)
    {
        _surface.UseGradient = fillEnd is not null && !isDisabled;
        if (fillEnd is Color end)
        {
            _surface.Color2 = end;
        }
    }
}
