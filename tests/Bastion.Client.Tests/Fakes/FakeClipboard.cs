using System;
using Bastion.Client.Screens;

namespace Bastion.Client.Tests.Fakes;

public sealed class FakeClipboard : IClipboard
{
    public FakeClipboard()
    {
        IsAvailable = true;
    }

    public bool IsAvailable { get; set; }

    public bool TryCopyText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return IsAvailable;
    }
}
