using System;

namespace Bastion.Client.Theme;

[Flags]
public enum InteractionFlags
{
    None = 0,
    Hovered = 1,
    Pressed = 2,
    Focused = 4,
    Disabled = 8,
    Checked = 16,
}
