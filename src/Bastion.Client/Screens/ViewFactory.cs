using System;
using System.Globalization;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.Converters;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
using Gum.Wireframe;
using RenderingLibrary.Graphics;

namespace Bastion.Client.Screens;

public static class ViewFactory
{
    private const float VersionMarginY = 16;
    private const int VersionFieldCount = 3;
    private const float MiddleOffsetY = 0;
    private const float MiddleOffsetX = 0;

    public static Label CreateLabel(string textKey, TextStyle style)
    {
        ArgumentNullException.ThrowIfNull(textKey);
        ArgumentNullException.ThrowIfNull(style);

        var label = new Label
        {
            Text = textKey,
        };
        var labelVisual = (LabelVisual)label.Visual;
        style.ApplyTo(labelVisual);
        return label;
    }

    public static Label CreateBoundLabel(string viewModelProperty, TextStyle style, float width)
    {
        ArgumentNullException.ThrowIfNull(viewModelProperty);
        ArgumentNullException.ThrowIfNull(style);

        Label label = CreateFixedWidthLabel(style, width);
        label.SetBinding(nameof(Label.Text), viewModelProperty);
        return label;
    }

    public static Label CreateBoundUntranslatedLabel(string viewModelProperty, TextStyle style, float width)
    {
        ArgumentNullException.ThrowIfNull(viewModelProperty);
        ArgumentNullException.ThrowIfNull(style);

        Label label = CreateFixedWidthLabel(style, width);
        DisableTranslation(label);
        label.SetBinding(nameof(Label.Text), viewModelProperty);
        return label;
    }

    public static Label CreateBoundUntranslatedLabel(string viewModelProperty, TextStyle style)
    {
        ArgumentNullException.ThrowIfNull(viewModelProperty);
        ArgumentNullException.ThrowIfNull(style);

        Label label = CreateLabel(string.Empty, style);
        DisableTranslation(label);
        label.SetBinding(nameof(Label.Text), viewModelProperty);
        return label;
    }

    public static Label CreateBoundNotice(string textProperty, string visibilityProperty, TextStyle style)
    {
        ArgumentNullException.ThrowIfNull(textProperty);
        ArgumentNullException.ThrowIfNull(visibilityProperty);
        ArgumentNullException.ThrowIfNull(style);

        Label label = CreateBoundUntranslatedLabel(textProperty, style);
        label.SetBinding(nameof(Label.IsVisible), visibilityProperty);
        return label;
    }

    public static Label CreateUntranslatedLabel(string text, TextStyle style)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(style);

        Label label = CreateLabel(string.Empty, style);
        DisableTranslation(label);
        label.Text = text;
        return label;
    }

    public static Button CreateButton(string textKey, float width, float height)
    {
        ArgumentNullException.ThrowIfNull(textKey);

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
        ArgumentNullException.ThrowIfNull(element);

        element.YUnits = GeneralUnitType.PixelsFromMiddle;
        element.YOrigin = VerticalAlignment.Center;
        element.Y = MiddleOffsetY;
    }

    public static void CenterHorizontally(FrameworkElement element)
    {
        ArgumentNullException.ThrowIfNull(element);

        element.XUnits = GeneralUnitType.PixelsFromMiddle;
        element.XOrigin = HorizontalAlignment.Center;
        element.X = MiddleOffsetX;
    }

    public static TList CreateScrollList<TList>(float width, float height)
        where TList : ScrollViewer, new()
    {
        var list = new TList
        {
            Width = width,
            WidthUnits = DimensionUnitType.Absolute,
            Height = height,
            HeightUnits = DimensionUnitType.Absolute,
        };
        Shapes.HideDefaultParts(list);
        return list;
    }

    public static void ClearRows(GraphicalUiElement container)
    {
        ArgumentNullException.ThrowIfNull(container);

        while (container.Children.Count > 0)
        {
            container.Children.RemoveAt(container.Children.Count - 1);
        }
    }

    public static TRow RequireRow<TRow>(object? item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (item is not TRow row)
        {
            throw new ArgumentException($"The list item must be a {typeof(TRow).Name}.", nameof(item));
        }

        return row;
    }

    public static Label CreateVersionLabel()
    {
        string version = typeof(ViewFactory).Assembly.GetName().Version?.ToString(VersionFieldCount) ?? string.Empty;
        string text = string.Format(CultureInfo.CurrentCulture, Strings.AppVersionFormat, version);

        Label label = CreateLabel(string.Empty, TextStyles.Version);
        label.SetTextNoTranslate(text);
        label.Anchor(Gum.Wireframe.Anchor.BottomRight);
        label.X = -ScreenMetrics.MarginX;
        label.Y = -VersionMarginY;
        return label;
    }

    private static Label CreateFixedWidthLabel(TextStyle style, float width)
    {
        var label = new Label
        {
            Width = width,
            WidthUnits = DimensionUnitType.Absolute,
        };
        var labelVisual = (LabelVisual)label.Visual;
        style.ApplyTo(labelVisual);
        return label;
    }

    private static void DisableTranslation(Label label)
    {
        var labelVisual = (LabelVisual)label.Visual;
        labelVisual.LocalizeText = false;
    }
}
