using System.Collections.Generic;

namespace Bastion.Client.Localization;

public interface ILanguageService
{
    IReadOnlyList<SupportedLanguage> Languages { get; }

    SupportedLanguage Current { get; }

    void Start();

    void Change(SupportedLanguage language);
}
