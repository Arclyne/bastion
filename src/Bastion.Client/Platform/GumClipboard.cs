using System;
using Bastion.Client.Screens;
using Gum.Forms.Controls;

namespace Bastion.Client.Platform;

public sealed class GumClipboard : IClipboard
{
    private readonly IGumClipboard? _clipboard;

    public GumClipboard(IGumClipboard? clipboard)
    {
        _clipboard = clipboard;
    }

    public bool TryCopyText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (_clipboard is null)
        {
            return false;
        }

        _clipboard.SetText(text);
        return true;
    }
}
