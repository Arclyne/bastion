using System.Collections.Generic;
using System.Globalization;
using System;
using Bastion.Domain.Common;

namespace Bastion.Client.Localization;

public static class LanguageCatalog
{
    private static readonly SupportedLanguage _spanish = new SupportedLanguage("es-MX", Strings.LanguageNameSpanish);

    public static SupportedLanguage English { get; } = new SupportedLanguage("en-US", Strings.LanguageNameEnglish);

    public static SupportedLanguage Default { get; } = English;

    public static IReadOnlyList<SupportedLanguage> Languages { get; } = new[] { _spanish, English };

    public static Lookup<SupportedLanguage> FindByCultureName(string cultureName)
    {
        ArgumentNullException.ThrowIfNull(cultureName);

        foreach (SupportedLanguage language in Languages)
        {
            if (string.Equals(language.CultureName, cultureName, StringComparison.OrdinalIgnoreCase))
            {
                return Lookup.Found(language);
            }
        }

        return Lookup.Missing<SupportedLanguage>($"CultureName={cultureName}");
    }

    public static Lookup<SupportedLanguage> FindClosest(CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);

        Lookup<SupportedLanguage> exactMatch = FindByCultureName(culture.Name);
        if (exactMatch.IsFound)
        {
            return exactMatch;
        }

        foreach (SupportedLanguage language in Languages)
        {
            string languageCode = new CultureInfo(language.CultureName).TwoLetterISOLanguageName;
            if (languageCode == culture.TwoLetterISOLanguageName)
            {
                return Lookup.Found(language);
            }
        }

        return Lookup.Missing<SupportedLanguage>($"CultureName={culture.Name}");
    }
}
