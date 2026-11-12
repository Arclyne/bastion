using System;
using Bastion.Client.Localization;

namespace Bastion.Client.Tests.Fakes;

public sealed class FakeLanguagePreferenceStore : ILanguagePreferenceStore
{
    private readonly string? _savedCultureName;

    public FakeLanguagePreferenceStore(string? savedCultureName)
    {
        _savedCultureName = savedCultureName;
    }

    public string? LoadCultureName()
    {
        return _savedCultureName;
    }

    public void SaveCultureName(string cultureName)
    {
        ArgumentNullException.ThrowIfNull(cultureName);
    }
}
