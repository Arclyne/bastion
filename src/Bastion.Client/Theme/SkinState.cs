using System;

namespace Bastion.Client.Theme;

public sealed class SkinState
{
    private readonly InteractiveSkin _skin;
    private readonly InteractionStates _flags;

    public SkinState(InteractiveSkin skin, InteractionStates flags)
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
