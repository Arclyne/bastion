using System;
using System.Collections.Generic;
using System.Globalization;

namespace Bastion.Client.Localization;

public static class SupportedLanguages
{
    public static SupportedLanguage Spanish { get; } = new SupportedLanguage("es-MX", Strings.LanguageNameSpanish);

    public static SupportedLanguage English { get; } = new SupportedLanguage("en-US", Strings.LanguageNameEnglish);

    public static SupportedLanguage Default => English;

    public static IReadOnlyList<SupportedLanguage> All { get; } = new[] { Spanish, English };

    public static SupportedLanguage? FindByCultureName(string? cultureName)
    {
        foreach (SupportedLanguage language in All)
        {
            if (string.Equals(language.CultureName, cultureName, StringComparison.OrdinalIgnoreCase))
            {
                return language;
            }
        }

        return null;
    }

    public static SupportedLanguage? FindClosest(CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);

        SupportedLanguage? exactMatch = FindByCultureName(culture.Name);
        if (exactMatch is not null)
        {
            return exactMatch;
        }

        foreach (SupportedLanguage language in All)
        {
            string languageCode = new CultureInfo(language.CultureName).TwoLetterISOLanguageName;
            if (languageCode == culture.TwoLetterISOLanguageName)
            {
                return language;
            }
        }

        return null;
    }
}
