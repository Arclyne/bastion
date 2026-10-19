using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using Gum.Localization;

namespace Bastion.Client.Localization;

public sealed class LanguageService : ILanguageService
{
    private readonly ILanguagePreferenceStore _preferences;
    private readonly ILocalizationService _textDatabase;

    public LanguageService(ILanguagePreferenceStore preferences, ILocalizationService textDatabase)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        ArgumentNullException.ThrowIfNull(textDatabase);

        _preferences = preferences;
        _textDatabase = textDatabase;
        Current = SupportedLanguages.Default;
    }

    public IReadOnlyList<SupportedLanguage> Languages => SupportedLanguages.All;

    public SupportedLanguage Current { get; private set; }

    public void Start()
    {
        LoadTextDatabase();
        Apply(ResolveInitialLanguage());
    }

    public void Change(SupportedLanguage language)
    {
        ArgumentNullException.ThrowIfNull(language);

        Apply(language);
        _preferences.Save(language.CultureName);
    }

    private SupportedLanguage ResolveInitialLanguage()
    {
        SupportedLanguage? savedLanguage = SupportedLanguages.FindByCultureName(_preferences.Load());
        SupportedLanguage? systemLanguage = SupportedLanguages.FindClosest(CultureInfo.CurrentUICulture);
        return savedLanguage ?? systemLanguage ?? SupportedLanguages.Default;
    }

    private void Apply(SupportedLanguage language)
    {
        var culture = new CultureInfo(language.CultureName);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        Current = language;
        _textDatabase.CurrentLanguage = IndexOf(language);
    }

    private void LoadTextDatabase()
    {
        var headers = new List<string>();
        foreach (SupportedLanguage language in Languages)
        {
            headers.Add(language.CultureName);
        }

        var entries = new Dictionary<string, string[]>();
        foreach (string key in ReadKeys())
        {
            entries[key] = Translate(key);
        }

        _textDatabase.AddDatabase(entries, headers);
    }

    private string[] Translate(string key)
    {
        string[] translations = new string[Languages.Count];
        for (int languageIndex = 0; languageIndex < Languages.Count; languageIndex++)
        {
            var culture = new CultureInfo(Languages[languageIndex].CultureName);
            translations[languageIndex] = Strings.ResourceManager.GetString(key, culture) ?? key;
        }

        return translations;
    }

    private static List<string> ReadKeys()
    {
        var keys = new List<string>();
        ResourceSet? neutralSet = Strings.ResourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, true);
        if (neutralSet is null)
        {
            return keys;
        }

        foreach (DictionaryEntry entry in neutralSet)
        {
            keys.Add((string)entry.Key);
        }

        return keys;
    }

    private int IndexOf(SupportedLanguage language)
    {
        for (int languageIndex = 0; languageIndex < Languages.Count; languageIndex++)
        {
            if (Languages[languageIndex] == language)
            {
                return languageIndex;
            }
        }

        throw new ArgumentException("The language is not supported.", nameof(language));
    }
}
