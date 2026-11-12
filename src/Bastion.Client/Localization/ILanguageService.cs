using System;
using System.Collections.Generic;

namespace Bastion.Client.Localization;

public interface ILanguageService
{
    IReadOnlyList<SupportedLanguage> Languages { get; }

    SupportedLanguage CurrentLanguage { get; }

    event EventHandler? LanguageChanged;

    void LoadInitialLanguage();

    void ChangeLanguage(SupportedLanguage language);
}
