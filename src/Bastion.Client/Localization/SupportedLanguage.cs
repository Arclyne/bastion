namespace Bastion.Client.Localization;

public sealed record SupportedLanguage(string CultureName, string DisplayName)
{
    public override string ToString()
    {
        return DisplayName;
    }
}
