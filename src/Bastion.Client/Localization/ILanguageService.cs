using System.Collections.Generic;

namespace Bastion.Client.Localization;

public interface ILanguageService
{
    IReadOnlyList<SupportedLanguage> Languages { get; }

    SupportedLanguage CurrentLanguage { get; }

    void LoadInitialLanguage();

    void ChangeLanguage(SupportedLanguage language);
}
