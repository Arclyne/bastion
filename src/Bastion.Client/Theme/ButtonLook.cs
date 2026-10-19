using Microsoft.Xna.Framework;

namespace Bastion.Client.Theme;

public sealed class ButtonLook
{
    public required Color Fill { get; init; }

    public Color? FillEnd { get; init; }

    public required Color HoverFill { get; init; }

    public Color? HoverFillEnd { get; init; }

    public Color SelectedFill { get; init; }

    public Color? Border { get; init; }

    public Color? HoverBorder { get; init; }

    public required Color ShadowColor { get; init; }

    public float ShadowOffsetY { get; init; }

    public float ShadowBlur { get; init; }

    public bool ShouldSinkWhenPressed { get; init; }

    public required float Radius { get; init; }

    public required TextStyle Text { get; init; }
}
