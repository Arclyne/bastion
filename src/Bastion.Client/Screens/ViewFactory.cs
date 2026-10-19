using System.Globalization;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;

namespace Bastion.Client.Screens;

public static class ViewFactory
{
    private const float ScreenMarginX = 24;
    private const float VersionMarginY = 16;
    private const int VersionFieldCount = 3;

    public static Label CreateLabel(string textKey, TextStyle style)
    {
        var label = new Label { Text = textKey };
        style.ApplyTo((LabelVisual)label.Visual);
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
}
