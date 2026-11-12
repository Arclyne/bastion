using System;

namespace Bastion.Client.Localization;

public sealed record SupportedLanguage
{
    public SupportedLanguage(string cultureName, string displayName)
    {
        ArgumentNullException.ThrowIfNull(cultureName);
        ArgumentNullException.ThrowIfNull(displayName);

        CultureName = cultureName;
        DisplayName = displayName;
    }

    public string CultureName { get; }

    public string DisplayName { get; }

    public override string ToString()
    {
        return DisplayName;
    }
}
