using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using Gum.Localization;

namespace Bastion.Client.Localization;

/// <inheritdoc/>
public sealed class LanguageService : ILanguageService
{
    private readonly ILanguagePreferenceStore _languagePreferenceStore;
    private readonly ILocalizationService _textDatabase;

    public LanguageService(ILanguagePreferenceStore languagePreferenceStore, ILocalizationService textDatabase)
    {
        ArgumentNullException.ThrowIfNull(languagePreferenceStore);
        ArgumentNullException.ThrowIfNull(textDatabase);

        _languagePreferenceStore = languagePreferenceStore;
        _textDatabase = textDatabase;
        Languages = LanguageCatalog.Languages;
        CurrentLanguage = LanguageCatalog.Default;
    }

    /// <inheritdoc/>
    public IReadOnlyList<SupportedLanguage> Languages { get; }

    /// <inheritdoc/>
    public SupportedLanguage CurrentLanguage { get; private set; }

    /// <inheritdoc/>
    public event EventHandler? LanguageChanged;

    /// <inheritdoc/>
    public void LoadInitialLanguage()
    {
        LoadTextDatabase();
        ApplyLanguage(ResolveInitialLanguage());
    }

    /// <inheritdoc/>
    public void ChangeLanguage(SupportedLanguage language)
    {
        ArgumentNullException.ThrowIfNull(language);

        ApplyLanguage(language);
        _languagePreferenceStore.SaveCultureName(language.CultureName);
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
            entries[key] = ReadTranslations(key);
        }

        _textDatabase.AddDatabase(entries, headers);
    }

    private static List<string> ReadKeys()
    {
        var keys = new List<string>();
        ResourceSet? neutralSet = Strings.ResourceManager.GetResourceSet(
            CultureInfo.InvariantCulture,
            createIfNotExists: true,
            tryParents: true);
        if (neutralSet is null)
        {
            return keys;
        }

        foreach (DictionaryEntry entry in neutralSet)
        {
            string key = (string)entry.Key;
            keys.Add(key);
        }

        return keys;
    }

    private string[] ReadTranslations(string key)
    {
        string[] translations = new string[Languages.Count];
        for (int languageIndex = 0; languageIndex < Languages.Count; languageIndex++)
        {
            var culture = new CultureInfo(Languages[languageIndex].CultureName);
            translations[languageIndex] = Strings.ResourceManager.GetString(key, culture) ?? key;
        }

        return translations;
    }

    private SupportedLanguage ResolveInitialLanguage()
    {
        SupportedLanguage? savedLanguage = LanguageCatalog.FindByCultureName(_languagePreferenceStore.LoadCultureName());
        SupportedLanguage? systemLanguage = LanguageCatalog.FindClosest(CultureInfo.CurrentUICulture);
        return savedLanguage ?? systemLanguage ?? LanguageCatalog.Default;
    }

    private void ApplyLanguage(SupportedLanguage language)
    {
        var culture = new CultureInfo(language.CultureName);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        Strings.Culture = culture;

        CurrentLanguage = language;
        _textDatabase.CurrentLanguage = IndexOf(language);
        LanguageChanged?.Invoke(this, EventArgs.Empty);
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
