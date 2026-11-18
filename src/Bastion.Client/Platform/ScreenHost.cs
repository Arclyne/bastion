using System;
using Bastion.Client.Controls;
using Bastion.Client.Screens;
using Gum.Wireframe;

namespace Bastion.Client.Platform;

public sealed class ScreenHost : IScreenHost
{
    private readonly InteractiveGue _root;
    private readonly InteractiveGue _modalRoot;
    private ScreenView? _shownScreen;

    public ScreenHost(InteractiveGue root, InteractiveGue modalRoot)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(modalRoot);

        _root = root;
        _modalRoot = modalRoot;
    }

    public void ShowScreen(ScreenView screen)
    {
        ArgumentNullException.ThrowIfNull(screen);

        RemoveShownScreen();
        _shownScreen = screen;
        foreach (ModalLayer dialogModalLayer in screen.Dialogs)
        {
            dialogModalLayer.IsOpenChanged += HandleDialogIsOpenChanged;
            PlaceDialog(dialogModalLayer);
        }

        _root.AddChild(screen);
    }

    private void RemoveShownScreen()
    {
        if (_shownScreen is null)
        {
            return;
        }

        foreach (ModalLayer dialogModalLayer in _shownScreen.Dialogs)
        {
            dialogModalLayer.IsOpenChanged -= HandleDialogIsOpenChanged;
        }

        _root.Children.Remove(_shownScreen.Visual);
        _modalRoot.Children.Clear();
        InteractiveGue.CurrentInputReceiver = null;
    }

    private void HandleDialogIsOpenChanged(object? sender, EventArgs e)
    {
        if (sender is not ModalLayer dialogModalLayer)
        {
            return;
        }

        PlaceDialog(dialogModalLayer);
    }

    private void PlaceDialog(ModalLayer dialogModalLayer)
    {
        if (!dialogModalLayer.IsOpen)
        {
            _modalRoot.Children.Remove(dialogModalLayer.Visual);
            return;
        }

        dialogModalLayer.Visual.RefreshLocalization();
        _modalRoot.AddChild(dialogModalLayer);
    }
}
