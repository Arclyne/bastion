using Bastion.Client.Theme;
using Gum.Forms.Controls;

namespace Bastion.Client.Controls;

public sealed class PasswordField : PasswordBox
{
    private readonly FieldSkin _skin;

    public PasswordField()
    {
        _skin = FieldSkin.CreateAndAttach(this);
    }

    public bool HasError
    {
        get => _skin.HasError;
        set => _skin.HasError = value;
    }
}
