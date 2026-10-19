namespace Bastion.Client.Localization;

public interface ILanguagePreferenceStore
{
    string? Load();

    void Save(string cultureName);
}
