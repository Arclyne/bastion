using System;
using Bastion.Client.Localization;

namespace Bastion.Client.Tests.Fakes;

public sealed class FakeLanguagePreferenceStore : ILanguagePreferenceStore
{
    private readonly string _savedCultureName;

    public FakeLanguagePreferenceStore(string savedCultureName)
    {
        _savedCultureName = savedCultureName;
    }

    public Exception? ReadFailure { get; set; }

    public string LoadCultureName()
    {
        if (ReadFailure is not null)
        {
            throw ReadFailure;
        }

        return _savedCultureName;
    }

    public void SaveCultureName(string cultureName)
    {
    }
}
