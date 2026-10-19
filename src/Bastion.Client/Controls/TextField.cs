using Bastion.Client.Theme;
using Gum.Forms.Controls;

namespace Bastion.Client.Controls;

public sealed class TextField : TextBox
{
    private readonly FieldSkin _skin;

    public TextField()
    {
        _skin = FieldSkin.Apply(this);
    }

    public bool HasError
    {
        get => _skin.HasError;
        set => _skin.HasError = value;
    }
}
