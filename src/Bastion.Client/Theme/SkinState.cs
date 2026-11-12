using System;

namespace Bastion.Client.Theme;

public sealed class SkinState
{
    private readonly InteractiveSkin _skin;
    private readonly InteractionFlags _flags;

    public SkinState(InteractiveSkin skin, InteractionFlags flags)
    {
        ArgumentNullException.ThrowIfNull(skin);

        _skin = skin;
        _flags = flags;
    }

    public void Show()
    {
        _skin.ShowState(_flags);
    }
}
