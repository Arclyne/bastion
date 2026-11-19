using System;
using Bastion.Client.Screens;

namespace Bastion.Client.Tests.Fakes;

public sealed class FakeClipboard : IClipboard
{
    public FakeClipboard()
    {
        CanCopyText = true;
    }

    public bool CanCopyText { get; set; }

    public void CopyText(string text)
    {
        if (!CanCopyText)
        {
            throw new InvalidOperationException("Text cannot be copied because there is no clipboard.");
        }
    }
}
