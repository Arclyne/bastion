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

        float textWidth = width - (PaddingX * 2);
        _title = CreateLabel(TextStyles.NoticeTitle, textWidth);
        _message = CreateLabel(TextStyles.NoticeMessage, textWidth);
        var content = new StackPanel { X = PaddingX, Y = PaddingY, Spacing = TitleSpacing };
        content.AddChild(_title);
        content.AddChild(_message);
        AddChild(content);

        _surface = Shapes.CreateRounded(Palette.Card, Radius);
        _surface.StrokeWidth = BorderWidth;
        Shapes.InsertBehindContent(Visual, _surface);
        Tone = NoticeTone.None;
    }

    public NoticeTone Tone
    {
        get => _tone;
        set
        {
            _tone = value;
            Color accent = PickAccent(value);
            _surface.FillColor = PickFill(value);
            _surface.StrokeColor = new Color(accent.R, accent.G, accent.B, BorderAlpha);
            ((LabelVisual)_title.Visual).Color = accent;
        }
    }

    public string Title
    {
        get => _title.Text ?? string.Empty;
        set
        {
            string title = value ?? string.Empty;
            _title.SetTextNoTranslate(title);
            _title.IsVisible = title.Length > 0;
        }
    }

    public string Message
    {
        get => _message.Text ?? string.Empty;
        set => _message.SetTextNoTranslate(value ?? string.Empty);
    }

    private static Label CreateLabel(TextStyle style, float width)
    {
        var label = new Label { Width = width, WidthUnits = DimensionUnitType.Absolute };
        style.ApplyTo((LabelVisual)label.Visual);
        return label;
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
            default:
                return Palette.Card;
        }
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
            default:
                return Palette.Border;
        }
    }
}
