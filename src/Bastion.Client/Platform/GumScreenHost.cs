using System;
using Bastion.Client.Controls;
using Bastion.Client.Screens;
using Gum.Wireframe;

namespace Bastion.Client.Platform;

public sealed class GumScreenHost : IScreenHost
{
    private readonly InteractiveGue _root;
    private readonly InteractiveGue _modalRoot;
    private ScreenView? _shownScreen;

    public GumScreenHost(InteractiveGue root, InteractiveGue modalRoot)
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
        foreach (ModalLayer dialog in screen.Dialogs)
        {
            dialog.IsOpenChanged += HandleDialogIsOpenChanged;
            PlaceDialog(dialog);
        }

        _root.AddChild(screen);
    }

    private void RemoveShownScreen()
    {
        if (_shownScreen is null)
        {
            return;
        }

        foreach (ModalLayer dialog in _shownScreen.Dialogs)
        {
            dialog.IsOpenChanged -= HandleDialogIsOpenChanged;
        }

        _root.Children.Remove(_shownScreen.Visual);
        _modalRoot.Children.Clear();
        InteractiveGue.CurrentInputReceiver = null;
    }

    private void HandleDialogIsOpenChanged(object? sender, EventArgs e)
    {
        if (sender is not ModalLayer dialog)
        {
            return;
        }

        PlaceDialog(dialog);
    }

    private void PlaceDialog(ModalLayer dialog)
    {
        if (!dialog.IsOpen)
        {
            _modalRoot.Children.Remove(dialog.Visual);
            return;
        }

        dialog.Visual.RefreshLocalization();
        _modalRoot.AddChild(dialog);
    }
}
