using System;
using Bastion.Client.Theme;
using Gum.Forms.Controls;

namespace Bastion.Client.Controls;

public sealed class ModalLayer : Panel
{
    private const float OverlayCornerRadius = 0;

    private bool _isOpen;

    public ModalLayer()
    {
        Dock(Gum.Wireframe.Dock.Fill);
        ShapeFactory.InsertBehindContent(Visual, ShapeFactory.CreateRounded(Palette.Overlay, OverlayCornerRadius));
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
            IsOpenChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? IsOpenChanged;
}
