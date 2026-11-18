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
    private readonly Label _titleLabel;
    private readonly Label _messageLabel;
    private NoticeTone _tone;

    public NoticeBox(float width)
    {
        Width = width;
        WidthUnits = DimensionUnitType.Absolute;
        Height = PaddingY;
        HeightUnits = DimensionUnitType.RelativeToChildren;

        float textWidth = width - (PaddingX * HorizontalSideCount);
        _titleLabel = TextStyleCatalog.NoticeTitle.CreateFixedWidthLabel(textWidth);
        _messageLabel = TextStyleCatalog.NoticeMessage.CreateFixedWidthLabel(textWidth);
        var contentStackPanel = new StackPanel
        {
            X = PaddingX,
            Y = PaddingY,
            Spacing = TitleSpacing,
        };
        contentStackPanel.AddChild(_titleLabel);
        contentStackPanel.AddChild(_messageLabel);
        AddChild(contentStackPanel);

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
        get => _titleLabel.Text ?? string.Empty;
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
        get => _messageLabel.Text ?? string.Empty;
        set
        {
            string message = value ?? string.Empty;
            if (string.Equals(message, Message, StringComparison.Ordinal))
            {
                return;
            }

            _messageLabel.SetTextNoTranslate(message);
        }
    }

    private void ShowTone(NoticeTone tone)
    {
        Color accent = PickAccent(tone);
        _surface.FillColor = PickFill(tone);
        _surface.StrokeColor = new Color(accent.R, accent.G, accent.B, BorderAlpha);
        var titleVisual = (LabelVisual)_titleLabel.Visual;
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
        _titleLabel.SetTextNoTranslate(title);
        _titleLabel.IsVisible = title.Length > 0;
    }
}
