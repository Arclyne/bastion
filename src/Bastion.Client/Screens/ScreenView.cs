using System;
using System.Collections.Generic;
using Bastion.Client.Controls;
using Gum.Forms.Controls;

namespace Bastion.Client.Screens;

public abstract class ScreenView : Panel
{
    private readonly List<ModalLayer> _dialogs = new List<ModalLayer>();

    protected ScreenView()
    {
        Dock(Gum.Wireframe.Dock.Fill);
    }

    public IReadOnlyList<ModalLayer> Dialogs => _dialogs;

    protected void AddDialog(FrameworkElement content, object viewModel, string isOpenProperty)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(isOpenProperty);

        var dialog = new ModalLayer
        {
            BindingContext = viewModel,
        };
        dialog.SetBinding(nameof(ModalLayer.IsOpen), isOpenProperty);
        dialog.AddChild(content);
        _dialogs.Add(dialog);
    }
}
