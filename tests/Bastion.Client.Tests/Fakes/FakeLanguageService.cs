using System;
using System.Collections.Generic;
using Bastion.Client.Localization;

namespace Bastion.Client.Tests.Fakes;

public sealed class FakeLanguageService : ILanguageService
{
    public FakeLanguageService()
    {
        Languages = LanguageCatalog.Languages;
        CurrentLanguage = LanguageCatalog.Default;
    }

    public IReadOnlyList<SupportedLanguage> Languages { get; }

    public SupportedLanguage CurrentLanguage { get; private set; }

    public event EventHandler? LanguageChanged;

    public void LoadInitialLanguage()
    {
    }

    public void ChangeLanguage(SupportedLanguage language)
    {
        ArgumentNullException.ThrowIfNull(language);

        CurrentLanguage = language;
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }
}
