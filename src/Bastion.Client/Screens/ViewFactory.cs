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
    private const float RetryButtonWidth = 140;
    private const float RetryButtonHeight = 40;
    private const float LoadFailureSpacing = 10;
    private const float VersionMarginY = 16;

    public static Label CreateLabel(string textKey, TextStyle style)
    {
        ArgumentNullException.ThrowIfNull(textKey);
        ArgumentNullException.ThrowIfNull(style);

        var label = new Label
        {
            Text = textKey,
        };
        style.ApplyTo(label);
        return label;
    }

    public static Label CreateBoundLabel(string viewModelProperty, TextStyle style, float width)
    {
        ArgumentNullException.ThrowIfNull(viewModelProperty);
        ArgumentNullException.ThrowIfNull(style);

        Label label = style.CreateFixedWidthLabel(width);
        label.SetBinding(nameof(Label.Text), viewModelProperty);
        return label;
    }

    public static Label CreateBoundUntranslatedLabel(string viewModelProperty, TextStyle style, float width)
    {
        ArgumentNullException.ThrowIfNull(viewModelProperty);
        ArgumentNullException.ThrowIfNull(style);

        Label label = style.CreateFixedWidthLabel(width);
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

    public static Label CreateToggledLabel(string textKey, string visibilityProperty, TextStyle style)
    {
        ArgumentNullException.ThrowIfNull(textKey);
        ArgumentNullException.ThrowIfNull(visibilityProperty);
        ArgumentNullException.ThrowIfNull(style);

        Label label = CreateLabel(textKey, style);
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

    public static Button CreateBoundButton(string textProperty, float width, float height)
    {
        ArgumentNullException.ThrowIfNull(textProperty);

        Button button = CreateButton(string.Empty, width, height);
        button.SetBinding(nameof(Button.Text), textProperty);
        return button;
    }

    public static Button CreateBoundUntranslatedButton(string textProperty, float width, float height)
    {
        ArgumentNullException.ThrowIfNull(textProperty);

        Button button = CreateButton(string.Empty, width, height);
        var buttonVisual = (ButtonVisual)button.Visual;
        buttonVisual.TextInstance.LocalizeText = false;
        button.SetBinding(nameof(Button.Text), textProperty);
        return button;
    }

    public static StackPanel CreateLoadFailure(string messageKey, string retryKey, EventHandler handleRetryClick)
    {
        ArgumentNullException.ThrowIfNull(messageKey);
        ArgumentNullException.ThrowIfNull(retryKey);
        ArgumentNullException.ThrowIfNull(handleRetryClick);

        Button retryButton = CreateButton(retryKey, RetryButtonWidth, RetryButtonHeight);
        ButtonSkin.CreateAndAttach(retryButton, ButtonLookCatalog.Secondary);
        retryButton.Click += handleRetryClick;

        var failure = new StackPanel
        {
            Spacing = LoadFailureSpacing,
        };
        Label message = CreateLabel(messageKey, TextStyleCatalog.FieldError);
        failure.AddChild(message);
        failure.AddChild(retryButton);
        return failure;
    }

    public static Panel CreateToggledContainer(FrameworkElement content, string visibilityProperty)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(visibilityProperty);

        var container = new Panel
        {
            WidthUnits = DimensionUnitType.RelativeToChildren,
            HeightUnits = DimensionUnitType.RelativeToChildren,
        };
        container.AddChild(content);
        container.SetBinding(nameof(Panel.IsVisible), visibilityProperty);
        return container;
    }

    public static StackPanel CreateCenteredLines(Label first, Label second)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);

        var lines = new StackPanel
        {
            Spacing = PanelMetrics.TextSpacing,
        };
        lines.AddChild(first);
        lines.AddChild(second);
        CenterVertically(lines);
        return lines;
    }

    public static void CenterVertically(FrameworkElement element)
    {
        ArgumentNullException.ThrowIfNull(element);

        element.YUnits = GeneralUnitType.PixelsFromMiddle;
        element.YOrigin = VerticalAlignment.Center;
        element.Y = 0;
    }

    public static void SetFixedWidth(FrameworkElement element, float width)
    {
        ArgumentNullException.ThrowIfNull(element);

        element.Width = width;
        element.WidthUnits = DimensionUnitType.Absolute;
    }

    public static void CenterHorizontally(FrameworkElement element)
    {
        ArgumentNullException.ThrowIfNull(element);

        element.XUnits = GeneralUnitType.PixelsFromMiddle;
        element.XOrigin = HorizontalAlignment.Center;
        element.X = 0;
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
        ShapeFactory.HideDefaultParts(list);
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
        string version = typeof(ViewFactory).Assembly.GetName().Version?.ToString(fieldCount: 3) ?? string.Empty;
        string text = string.Format(CultureInfo.CurrentCulture, Strings.AppVersionFormat, version);

        Label label = CreateUntranslatedLabel(text, TextStyleCatalog.Version);
        label.Anchor(Gum.Wireframe.Anchor.BottomRight);
        label.X = -ScreenMetrics.MarginX;
        label.Y = -VersionMarginY;
        return label;
    }

    private static void DisableTranslation(Label label)
    {
        var labelVisual = (LabelVisual)label.Visual;
        labelVisual.LocalizeText = false;
    }
}
