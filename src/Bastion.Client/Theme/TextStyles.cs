namespace Bastion.Client.Theme;

public static class TextStyles
{
    public static TextStyle Title { get; } = new TextStyle(FontFamilies.Display, 34, Palette.Ink);

    public static TextStyle Tagline { get; } = new TextStyle(FontFamilies.Bold, 14, Palette.Muted);

    public static TextStyle FieldLabel { get; } = new TextStyle(FontFamilies.ExtraBold, 13, Palette.Body);

    public static TextStyle FieldText { get; } = new TextStyle(FontFamilies.Bold, 16, Palette.Ink);

    public static TextStyle Placeholder { get; } = new TextStyle(FontFamilies.SemiBold, 15, Palette.Placeholder);

    public static TextStyle FieldError { get; } = new TextStyle(FontFamilies.Bold, 13, Palette.Error);

    public static TextStyle PrimaryButton { get; } = new TextStyle(FontFamilies.ExtraBold, 16, Palette.White);

    public static TextStyle SecondaryButton { get; } = new TextStyle(FontFamilies.ExtraBold, 16, Palette.Ink);

    public static TextStyle SmallButton { get; } = new TextStyle(FontFamilies.ExtraBold, 14, Palette.Body);

    public static TextStyle Picker { get; } = new TextStyle(FontFamilies.ExtraBold, 13, Palette.Body);

    public static TextStyle Divider { get; } = new TextStyle(FontFamilies.Bold, 13, Palette.Muted);

    public static TextStyle Hint { get; } = new TextStyle(FontFamilies.SemiBold, 13, Palette.Muted);

    public static TextStyle NoticeTitle { get; } = new TextStyle(FontFamilies.ExtraBold, 14, Palette.Ink);

    public static TextStyle NoticeMessage { get; } = new TextStyle(FontFamilies.Bold, 14, Palette.Ink);

    public static TextStyle DialogTitle { get; } = new TextStyle(FontFamilies.Display, 21, Palette.Ink);

    public static TextStyle DialogMessage { get; } = new TextStyle(FontFamilies.SemiBold, 15, Palette.Body);

    public static TextStyle MenuTitle { get; } = new TextStyle(FontFamilies.Display, 30, Palette.Ink);

    public static TextStyle MenuTagline { get; } = new TextStyle(FontFamilies.Bold, 13, Palette.Muted);

    public static TextStyle HeroTitle { get; } = new TextStyle(FontFamilies.Display, 23, Palette.White);

    public static TextStyle HeroDetail { get; } = new TextStyle(FontFamilies.Bold, 14, Palette.HeroDetail);

    public static TextStyle IdentityName { get; } = new TextStyle(FontFamilies.ExtraBold, 17, Palette.Ink);

    public static TextStyle IdentityDetail { get; } = new TextStyle(FontFamilies.Bold, 13, Palette.Body);

    public static TextStyle SectionLabel { get; } = new TextStyle(FontFamilies.ExtraBold, 13, Palette.Muted);

    public static TextStyle Version { get; } = new TextStyle(FontFamilies.Bold, 12, Palette.Muted);
}
