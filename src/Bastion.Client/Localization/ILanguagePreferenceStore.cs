namespace Bastion.Client.Localization;

public interface ILanguagePreferenceStore
{
    string? LoadCultureName();

    void SaveCultureName(string cultureName);
}
