using System;
using Bastion.Client.Screens;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
using Gum.GueDeriving;
using Microsoft.Xna.Framework;

namespace Bastion.Client.Controls;

public sealed class NoticeBox : Panel
{
    private const float PaddingX = 13;
    private const float PaddingY = 11;
    private const float Radius = 12;
    private const float BorderWidth = 1.5f;
    private const float TitleSpacing = 2;
    private const byte BorderAlpha = 0x33;
    private const int HorizontalSideCount = 2;

    private readonly RectangleRuntime _surface;
    private readonly Label _title;
    private readonly Label _message;
    private NoticeTone _tone;

    public NoticeBox(float width)
    {
        Width = width;
        WidthUnits = DimensionUnitType.Absolute;
        Height = PaddingY;
        HeightUnits = DimensionUnitType.RelativeToChildren;

        float textWidth = width - (PaddingX * HorizontalSideCount);
        _title = TextStyleCatalog.NoticeTitle.CreateFixedWidthLabel(textWidth);
        _message = TextStyleCatalog.NoticeMessage.CreateFixedWidthLabel(textWidth);
        var content = new StackPanel
        {
            X = PaddingX,
            Y = PaddingY,
            Spacing = TitleSpacing,
        };
        content.AddChild(_title);
        content.AddChild(_message);
        AddChild(content);

        _surface = ShapeFactory.CreateRounded(Palette.Card, Radius);
        _surface.StrokeWidth = BorderWidth;
        ShapeFactory.InsertBehindContent(Visual, _surface);
        _tone = NoticeTone.None;
        ShowTone(_tone);
        ShowTitle(string.Empty);
    }

    public NoticeTone Tone
    {
        get => _tone;
        set
        {
            if (_tone == value)
            {
                return;
            }

            _tone = value;
            ShowTone(value);
        }
    }

    public string? Title
    {
        get => _title.Text ?? string.Empty;
        set
        {
            string title = value ?? string.Empty;
            if (string.Equals(title, Title, StringComparison.Ordinal))
            {
                return;
            }

            ShowTitle(title);
        }
    }

    public string? Message
    {
        get => _message.Text ?? string.Empty;
        set
        {
            string message = value ?? string.Empty;
            if (string.Equals(message, Message, StringComparison.Ordinal))
            {
                return;
            }

            _message.SetTextNoTranslate(message);
        }
    }

    private void ShowTone(NoticeTone tone)
    {
        Color accent = PickAccent(tone);
        _surface.FillColor = PickFill(tone);
        _surface.StrokeColor = new Color(accent.R, accent.G, accent.B, BorderAlpha);
        var titleVisual = (LabelVisual)_title.Visual;
        titleVisual.Color = accent;
    }

    private static Color PickAccent(NoticeTone tone)
    {
        switch (tone)
        {
            case NoticeTone.Error:
                return Palette.Error;
            case NoticeTone.Warning:
                return Palette.Warning;
            case NoticeTone.Success:
                return Palette.Success;
            case NoticeTone.None:
                return Palette.Border;
            default:
                throw new ArgumentOutOfRangeException(nameof(tone), tone, "Unsupported notice tone.");
        }
    }

    private static Color PickFill(NoticeTone tone)
    {
        switch (tone)
        {
            case NoticeTone.Error:
                return Palette.ErrorSoft;
            case NoticeTone.Warning:
                return Palette.WarningSoft;
            case NoticeTone.Success:
                return Palette.SuccessSoft;
            case NoticeTone.None:
                return Palette.Card;
            default:
                throw new ArgumentOutOfRangeException(nameof(tone), tone, "Unsupported notice tone.");
        }
    }

    private void ShowTitle(string title)
    {
        _title.SetTextNoTranslate(title);
        _title.IsVisible = title.Length > 0;
    }
}
