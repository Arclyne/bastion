using System;
using Bastion.Client.Localization;

namespace Bastion.Client.Screens;

public sealed record LocalServices
{
    public LocalServices(ILanguageService languages, IClipboard clipboard, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(languages);
        ArgumentNullException.ThrowIfNull(clipboard);
        ArgumentNullException.ThrowIfNull(timeProvider);

        Languages = languages;
        Clipboard = clipboard;
        TimeProvider = timeProvider;
    }

    public ILanguageService Languages { get; }

    public IClipboard Clipboard { get; }

    public TimeProvider TimeProvider { get; }
}
