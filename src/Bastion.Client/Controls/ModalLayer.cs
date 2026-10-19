using Bastion.Client.Theme;
using Gum;
using Gum.Forms.Controls;

namespace Bastion.Client.Controls;

public sealed class ModalLayer : Panel
{
    private bool _isOpen;

    public ModalLayer()
    {
        Dock(Gum.Wireframe.Dock.Fill);
        Shapes.InsertBehindContent(Visual, Shapes.CreateRounded(Palette.Overlay, 0));
    }

    public bool IsOpen
    {
        get => _isOpen;
        set
        {
            if (_isOpen == value)
            {
                return;
            }

            _isOpen = value;
            if (value)
            {
                GumService.Default.ModalRoot.AddChild(this);
            }
            else
            {
                GumService.Default.ModalRoot.Children.Remove(Visual);
            }
        }
    }
}
