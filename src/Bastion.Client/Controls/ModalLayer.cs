using System;
using Bastion.Client.Theme;
using Gum.Forms.Controls;
using Gum.GueDeriving;

namespace Bastion.Client.Controls;

public sealed class ModalLayer : Panel
{
    private const float OverlayCornerRadius = 0;

    private bool _isOpen;

    public ModalLayer()
    {
        Dock(Gum.Wireframe.Dock.Fill);
        RectangleRuntime overlay = ShapeFactory.CreateRounded(Palette.Overlay, OverlayCornerRadius);
        ShapeFactory.InsertBehindContent(Visual, overlay);
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
            OnIsOpenChanged();
        }
    }

    public event EventHandler? IsOpenChanged;

    private void OnIsOpenChanged()
    {
        IsOpenChanged?.Invoke(this, EventArgs.Empty);
    }
}
