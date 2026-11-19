using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Resources;
using Bastion.Domain.Common;
using Gum.Localization;
using log4net;

namespace Bastion.Client.Localization;

/// <inheritdoc/>
public sealed class LanguageService : ILanguageService
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(LanguageService));

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
        Lookup<SupportedLanguage> savedLanguage = FindSavedLanguage();
        if (savedLanguage.IsFound)
        {
            return savedLanguage.GetValue();
        }

        Lookup<SupportedLanguage> systemLanguage = LanguageCatalog.FindClosest(CultureInfo.CurrentUICulture);
        if (systemLanguage.IsFound)
        {
            return systemLanguage.GetValue();
        }

        return LanguageCatalog.Default;
    }

    private Lookup<SupportedLanguage> FindSavedLanguage()
    {
        try
        {
            string savedCultureName = _languagePreferenceStore.LoadCultureName();
            return LanguageCatalog.FindByCultureName(savedCultureName);
        }
        catch (UnauthorizedAccessException ex)
        {
            LogReadFailure(ex);
        }
        catch (IOException ex)
        {
            LogReadFailure(ex);
        }

        return Lookup.Missing<SupportedLanguage>("Search=SavedLanguage");
    }

    private static void LogReadFailure(Exception cause)
    {
        _logger.Warn("The saved language could not be read.", cause);
    }

    private void ApplyLanguage(SupportedLanguage language)
    {
        int languageIndex = FindLanguageIndex(language);
        var culture = new CultureInfo(language.CultureName);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        Strings.Culture = culture;

        CurrentLanguage = language;
        _textDatabase.CurrentLanguage = languageIndex;
        OnLanguageChanged();
    }

    private int FindLanguageIndex(SupportedLanguage language)
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

    private void OnLanguageChanged()
    {
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }
}
