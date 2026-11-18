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

    private readonly ButtonLook _buttonLook;
    private readonly RectangleRuntime _halo;
    private readonly RectangleRuntime _surface;
    private readonly TextRuntime _text;

    private ButtonSkin(ButtonBase button, ButtonLook buttonLook)
    {
        _buttonLook = buttonLook;
        _halo = ShapeFactory.CreateHalo(buttonLook.Radius);
        _surface = ShapeFactory.CreateRounded(buttonLook.Fill, buttonLook.Radius);
        _surface.StrokeWidth = buttonLook.Border is null ? 0 : BorderWidth;
        _surface.GradientY2 = ShapeFactory.FullPercentage;
        _surface.GradientY2Units = GeneralUnitType.Percentage;
        _surface.HasDropshadow = true;
        _surface.DropshadowColor = buttonLook.ShadowColor;
        _surface.DropshadowBlur = buttonLook.ShadowBlur;
        _text = FindText(button);
        buttonLook.Text.ApplyTo(_text);

        ShapeFactory.HideDefaultParts(button);
        ShapeFactory.InsertBehindContent(button.Visual, _halo, _surface);
    }

    public static ButtonSkin CreateAndAttach(ButtonBase button, ButtonLook buttonLook)
    {
        ArgumentNullException.ThrowIfNull(button);
        ArgumentNullException.ThrowIfNull(buttonLook);

        var skin = new ButtonSkin(button, buttonLook);
        skin.Attach(button);
        return skin;
    }

    public override void ShowState(InteractionStates flags)
    {
        bool isDisabled = flags.HasFlag(InteractionStates.Disabled);
        bool isHovered = flags.HasFlag(InteractionStates.Hovered);
        float sink = IsSinking(flags) ? _buttonLook.ShadowOffsetY : 0;

        _surface.FillColor = PickFill(flags);
        ApplyGradient(isHovered ? _buttonLook.HoverFillEnd : _buttonLook.FillEnd, isDisabled);
        _surface.StrokeColor = (isHovered ? _buttonLook.HoverBorder : _buttonLook.Border) ?? Color.Transparent;
        _surface.DropshadowOffsetY = _buttonLook.ShadowOffsetY - sink;
        _surface.Y = sink;
        _text.Y = sink;
        _text.Color = isDisabled ? Palette.Muted : _buttonLook.Text.Color;
        _halo.Y = sink;
        _halo.Visible = flags.HasFlag(InteractionStates.Focused) && !isDisabled;
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

    private bool IsSinking(InteractionStates flags)
    {
        return _buttonLook.ShouldSinkWhenPressed && flags.HasFlag(InteractionStates.Pressed);
    }

    private Color PickFill(InteractionStates flags)
    {
        if (flags.HasFlag(InteractionStates.Disabled))
        {
            return Palette.Disabled;
        }

        if (flags.HasFlag(InteractionStates.Checked))
        {
            return _buttonLook.SelectedFill;
        }

        if (flags.HasFlag(InteractionStates.Hovered))
        {
            return _buttonLook.HoverFill;
        }

        return _buttonLook.Fill;
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
