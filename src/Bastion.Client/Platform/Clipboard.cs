using System;
using Bastion.Client.Screens;
using Gum.Forms.Controls;

namespace Bastion.Client.Platform;

public sealed class Clipboard : IClipboard
{
    private readonly IGumClipboard? _clipboard;

    public Clipboard(IGumClipboard? clipboard)
    {
        _clipboard = clipboard;
    }

    public bool CanCopyText => _clipboard is not null;

    public void CopyText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (_clipboard is null)
        {
            throw new InvalidOperationException("Text cannot be copied because there is no clipboard.");
        }

        _clipboard.SetText(text);
    }
}
