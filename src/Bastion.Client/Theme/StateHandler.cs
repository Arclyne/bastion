namespace Bastion.Client.Theme;

public sealed class StateHandler
{
    private readonly InteractiveSkin _skin;
    private readonly InteractionFlags _flags;

    public StateHandler(InteractiveSkin skin, InteractionFlags flags)
    {
        _skin = skin;
        _flags = flags;
    }

    public void Apply()
    {
        _skin.Show(_flags);
    }
}
