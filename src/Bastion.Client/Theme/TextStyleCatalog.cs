namespace Bastion.Client.Theme;

public static class TextStyleCatalog
{
    public static TextStyle Title { get; } = new TextStyle(FontCatalog.Display, 34, Palette.Ink);

    public static TextStyle Tagline { get; } = new TextStyle(FontCatalog.Bold, 14, Palette.Muted);

    public static TextStyle FieldLabel { get; } = new TextStyle(FontCatalog.ExtraBold, 13, Palette.Body);

    public static TextStyle FieldText { get; } = new TextStyle(FontCatalog.Bold, 16, Palette.Ink);

    public static TextStyle Placeholder { get; } = new TextStyle(FontCatalog.SemiBold, 15, Palette.Placeholder);

    public static TextStyle FieldError { get; } = new TextStyle(FontCatalog.Bold, 13, Palette.Error);

    public static TextStyle PrimaryButton { get; } = new TextStyle(FontCatalog.ExtraBold, 16, Palette.White);

    public static TextStyle SecondaryButton { get; } = new TextStyle(FontCatalog.ExtraBold, 16, Palette.Ink);

    public static TextStyle SmallButton { get; } = new TextStyle(FontCatalog.ExtraBold, 14, Palette.Body);

    public static TextStyle Picker { get; } = new TextStyle(FontCatalog.ExtraBold, 13, Palette.Body);

    public static TextStyle Divider { get; } = new TextStyle(FontCatalog.Bold, 13, Palette.Muted);

    public static TextStyle Hint { get; } = new TextStyle(FontCatalog.SemiBold, 13, Palette.Muted);

    public static TextStyle NoticeTitle { get; } = new TextStyle(FontCatalog.ExtraBold, 14, Palette.Ink);

    public static TextStyle NoticeMessage { get; } = new TextStyle(FontCatalog.Bold, 14, Palette.Ink);

    public static TextStyle DialogTitle { get; } = new TextStyle(FontCatalog.Display, 21, Palette.Ink);

    public static TextStyle DialogMessage { get; } = new TextStyle(FontCatalog.SemiBold, 15, Palette.Body);

    public static TextStyle MenuTitle { get; } = new TextStyle(FontCatalog.Display, 30, Palette.Ink);

    public static TextStyle MenuTagline { get; } = new TextStyle(FontCatalog.Bold, 13, Palette.Muted);

    public static TextStyle HeroTitle { get; } = new TextStyle(FontCatalog.Display, 23, Palette.White);

    public static TextStyle HeroDetail { get; } = new TextStyle(FontCatalog.Bold, 14, Palette.HeroDetail);

    public static TextStyle IdentityName { get; } = new TextStyle(FontCatalog.ExtraBold, 17, Palette.Ink);

    public static TextStyle IdentityDetail { get; } = new TextStyle(FontCatalog.Bold, 13, Palette.Body);

    public static TextStyle SectionLabel { get; } = new TextStyle(FontCatalog.ExtraBold, 13, Palette.Muted);

    public static TextStyle ChatAuthor { get; } = new TextStyle(FontCatalog.ExtraBold, 13, Palette.Ink);

    public static TextStyle ChatOwnAuthor { get; } = new TextStyle(FontCatalog.ExtraBold, 13, Palette.Accent);

    public static TextStyle Version { get; } = new TextStyle(FontCatalog.Bold, 12, Palette.Muted);
}
