using System;
using System.Globalization;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Tests.Fakes;
using Gum.Localization;
using Xunit;

namespace Bastion.Client.Tests.Localization;

[Collection(nameof(GlobalCultureCollection))]
public sealed class TestLanguageService : IDisposable
{
    private const string SavedCultureName = "es-MX";

    public void Dispose()
    {
        Strings.Culture = null;
        CultureInfo.DefaultThreadCurrentCulture = null;
        CultureInfo.DefaultThreadCurrentUICulture = null;
    }

    [Fact]
    public async Task ChangeLanguage_InsideAsyncFlow_KeepsTextsInNewLanguageAfterward()
    {
        var preferenceStore = new FakeLanguagePreferenceStore(SavedCultureName);
        var localizationService = new LocalizationService();
        var languages = new LanguageService(preferenceStore, localizationService);
        languages.LoadInitialLanguage();
        var englishCulture = new CultureInfo(LanguageCatalog.English.CultureName);
        string? englishTagline = Strings.ResourceManager.GetString(nameof(Strings.LoginTagline), englishCulture);

        await ChangeLanguageAfterAwait(languages, LanguageCatalog.English);

        Assert.Equal(englishTagline, Strings.LoginTagline);
    }

    private static async Task ChangeLanguageAfterAwait(ILanguageService languages, SupportedLanguage language)
    {
        await Task.Yield();
        languages.ChangeLanguage(language);
    }
}
