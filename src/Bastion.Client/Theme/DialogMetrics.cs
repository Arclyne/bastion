namespace Bastion.Client.Theme;

public static class DialogMetrics
{
    public const float Width = 420;
    public const float PaddingX = 28;
    public const float PaddingTop = 26;
    public const float PaddingBottom = 24;
    public const float SectionSpacing = 14;
    public const float ButtonHeight = 46;
    public const float ContentWidth = Width - (PaddingX * HorizontalSideCount);

    private const int HorizontalSideCount = 2;
}
