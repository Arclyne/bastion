using System;
using System.Collections.Generic;
using Bastion.Client.Controls;
using Gum.Forms.Controls;

namespace Bastion.Client.Screens;

public abstract class ScreenView : Panel
{
    private readonly List<ModalLayer> _dialogs;

    protected ScreenView()
    {
        _dialogs = new List<ModalLayer>();
        Dock(Gum.Wireframe.Dock.Fill);
    }

    public IReadOnlyList<ModalLayer> Dialogs => _dialogs;

    protected void AddDialog(FrameworkElement content, object viewModel, string isOpenProperty)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(isOpenProperty);

        var dialogModalLayer = new ModalLayer
        {
            BindingContext = viewModel,
        };
        dialogModalLayer.SetBinding(nameof(ModalLayer.IsOpen), isOpenProperty);
        dialogModalLayer.AddChild(content);
        _dialogs.Add(dialogModalLayer);
    }
}
