using System;
using System.Globalization;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.Converters;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
using RenderingLibrary.Graphics;

namespace Bastion.Client.Screens;

public static class ViewFactory
{
    private const float ScreenMarginX = 24;
    private const float VersionMarginY = 16;
    private const int VersionFieldCount = 3;

    public static Label CreateLabel(string textKey, TextStyle style)
    {
        var label = new Label
        {
            Text = textKey,
        };
        style.ApplyTo((LabelVisual)label.Visual);
        return label;
    }

    public static Label CreateBoundLabel(string viewModelProperty, TextStyle style, float width)
    {
        ArgumentNullException.ThrowIfNull(viewModelProperty);

        Label label = CreateFixedWidthLabel(style, width);
        label.SetBinding(nameof(Label.Text), viewModelProperty);
        return label;
    }

    public static Label CreateBoundUntranslatedLabel(string viewModelProperty, TextStyle style, float width)
    {
        ArgumentNullException.ThrowIfNull(viewModelProperty);

        Label label = CreateFixedWidthLabel(style, width);
        ((LabelVisual)label.Visual).LocalizeText = false;
        label.SetBinding(nameof(Label.Text), viewModelProperty);
        return label;
    }

    public static Button CreateButton(string textKey, float width, float height)
    {
        return new Button
        {
            Text = textKey,
            Width = width,
            Height = height,
            HeightUnits = DimensionUnitType.Absolute,
        };
    }

    public static void CenterVertically(FrameworkElement element)
    {
        element.YUnits = GeneralUnitType.PixelsFromMiddle;
        element.YOrigin = VerticalAlignment.Center;
        element.Y = 0;
    }

    public static Label CreateVersionLabel()
    {
        string version = typeof(ViewFactory).Assembly.GetName().Version?.ToString(VersionFieldCount) ?? string.Empty;
        string text = string.Format(CultureInfo.CurrentCulture, Strings.AppVersionFormat, version);

        Label label = CreateLabel(string.Empty, TextStyles.Version);
        label.SetTextNoTranslate(text);
        label.Anchor(Gum.Wireframe.Anchor.BottomRight);
        label.X = -ScreenMarginX;
        label.Y = -VersionMarginY;
        return label;
    }

    private static Label CreateFixedWidthLabel(TextStyle style, float width)
    {
        ArgumentNullException.ThrowIfNull(style);

        var label = new Label
        {
            Width = width,
            WidthUnits = DimensionUnitType.Absolute,
        };
        style.ApplyTo((LabelVisual)label.Visual);
        return label;
    }
}
