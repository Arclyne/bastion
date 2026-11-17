using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System;
using Bastion.Client.Localization;
using Bastion.Client.Tests.Fakes;
using Gum.Localization;
using Xunit;

namespace Bastion.Client.Tests.Localization;

[Collection(nameof(GlobalCultureCollectionDefinition))]
public sealed class TestLanguageService : IDisposable
{
    private const string SavedCultureName = "es-MX";

    private readonly CultureInfo _originalUiCulture = CultureInfo.CurrentUICulture;

    public void Dispose()
    {
        Strings.Culture = null;
        CultureInfo.DefaultThreadCurrentCulture = null;
        CultureInfo.DefaultThreadCurrentUICulture = null;
        CultureInfo.CurrentUICulture = _originalUiCulture;
    }

    [Fact]
    public void LoadInitialLanguage_SavedLanguage_UsesSavedLanguage()
    {
        UseEnglishAsSystemLanguage();
        var preferenceStore = new FakeLanguagePreferenceStore(SavedCultureName);
        LanguageService languages = CreateLanguageService(preferenceStore);

        languages.LoadInitialLanguage();

        Assert.Equal(SavedCultureName, languages.CurrentLanguage.CultureName);
    }

    [Fact]
    public void LoadInitialLanguage_NoSavedLanguage_UsesSystemLanguage()
    {
        UseEnglishAsSystemLanguage();
        var preferenceStore = new FakeLanguagePreferenceStore(string.Empty);
        LanguageService languages = CreateLanguageService(preferenceStore);

        languages.LoadInitialLanguage();

        Assert.Equal(LanguageCatalog.English, languages.CurrentLanguage);
    }

    [Fact]
    public void LoadInitialLanguage_UnreadableSavedLanguage_UsesSystemLanguage()
    {
        UseEnglishAsSystemLanguage();
        var readFailure = new IOException();
        var preferenceStore = new FakeLanguagePreferenceStore(SavedCultureName) { ReadFailure = readFailure };
        LanguageService languages = CreateLanguageService(preferenceStore);

        languages.LoadInitialLanguage();

        Assert.Equal(LanguageCatalog.English, languages.CurrentLanguage);
    }

    [Fact]
    public async Task ChangeLanguage_InsideAsyncFlow_KeepsTextsInNewLanguageAfterward()
    {
        var preferenceStore = new FakeLanguagePreferenceStore(SavedCultureName);
        LanguageService languages = CreateLanguageService(preferenceStore);
        languages.LoadInitialLanguage();
        var englishCulture = new CultureInfo(LanguageCatalog.English.CultureName);
        string? englishTagline = Strings.ResourceManager.GetString(nameof(Strings.LoginTagline), englishCulture);

        await ChangeLanguageAfterAwait(languages, LanguageCatalog.English);

        Assert.Equal(englishTagline, Strings.LoginTagline);
    }

    private static void UseEnglishAsSystemLanguage()
    {
        var englishCulture = new CultureInfo(LanguageCatalog.English.CultureName);
        CultureInfo.CurrentUICulture = englishCulture;
    }

    private static LanguageService CreateLanguageService(FakeLanguagePreferenceStore preferenceStore)
    {
        var localizationService = new LocalizationService();
        return new LanguageService(preferenceStore, localizationService);
    }

    private static async Task ChangeLanguageAfterAwait(ILanguageService languages, SupportedLanguage language)
    {
        await Task.Yield();
        languages.ChangeLanguage(language);
    }
}
