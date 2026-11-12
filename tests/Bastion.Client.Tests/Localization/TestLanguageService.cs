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

    [Fact]
    public async Task ChangeLanguage_InsideAsyncFlow_KeepsTextsInNewLanguageAfterward()
    {
        var languages = new LanguageService(
            new FakeLanguagePreferenceStore(SavedCultureName),
            new LocalizationService());
        languages.LoadInitialLanguage();
        var englishCulture = new CultureInfo(LanguageCatalog.English.CultureName);
        string? englishTagline = Strings.ResourceManager.GetString(nameof(Strings.LoginTagline), englishCulture);

        await ChangeLanguageAfterAwait(languages, LanguageCatalog.English);

        Assert.Equal(englishTagline, Strings.LoginTagline);
    }

    public void Dispose()
    {
        Strings.Culture = null;
        CultureInfo.DefaultThreadCurrentCulture = null;
        CultureInfo.DefaultThreadCurrentUICulture = null;
    }

    private static async Task ChangeLanguageAfterAwait(ILanguageService languages, SupportedLanguage language)
    {
        await Task.Yield();
        languages.ChangeLanguage(language);
    }
}
