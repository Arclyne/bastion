namespace Bastion.Client.Theme;

public static class ButtonLooks
{
    private const float ButtonRadius = 14;
    private const float PillRadius = 21;
    private const float PrimaryEdge = 4;
    private const float SecondaryEdge = 2;
    private const float PillShadowOffsetY = 6;
    private const float PillShadowBlur = 16;
    private const float HeroRadius = 18;
    private const float HeroEdge = 5;

    public static ButtonLook Primary { get; } = new ButtonLook
    {
        Fill = Palette.AccentLight,
        FillEnd = Palette.Accent,
        HoverFill = Palette.AccentHover,
        HoverFillEnd = Palette.AccentHoverEnd,
        ShadowColor = Palette.AccentDark,
        ShadowOffsetY = PrimaryEdge,
        ShouldSinkWhenPressed = true,
        Radius = ButtonRadius,
        Text = TextStyles.PrimaryButton,
    };

    public static ButtonLook Hero { get; } = new ButtonLook
    {
        Fill = Palette.AccentLight,
        FillEnd = Palette.Accent,
        HoverFill = Palette.AccentHover,
        HoverFillEnd = Palette.AccentHoverEnd,
        ShadowColor = Palette.AccentDark,
        ShadowOffsetY = HeroEdge,
        ShouldSinkWhenPressed = true,
        Radius = HeroRadius,
        Text = TextStyles.HeroTitle,
    };

    public static ButtonLook Secondary { get; } = new ButtonLook
    {
        Fill = Palette.White,
        HoverFill = Palette.SurfaceHover,
        SelectedFill = Palette.Selected,
        Border = Palette.BorderStrong,
        HoverBorder = Palette.BorderHover,
        ShadowColor = Palette.ButtonEdge,
        ShadowOffsetY = SecondaryEdge,
        ShouldSinkWhenPressed = true,
        Radius = ButtonRadius,
        Text = TextStyles.SecondaryButton,
    };

    public static ButtonLook Pill { get; } = new ButtonLook
    {
        Fill = Palette.Card,
        HoverFill = Palette.White,
        Border = Palette.Border,
        HoverBorder = Palette.BorderHover,
        ShadowColor = Palette.Shadow,
        ShadowOffsetY = PillShadowOffsetY,
        ShadowBlur = PillShadowBlur,
        Radius = PillRadius,
        Text = TextStyles.SmallButton,
    };
}
