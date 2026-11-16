using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.Forms.Controls;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class RemoveFriendDialogView : DialogCard
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(RemoveFriendDialogView));

    private readonly FriendActionsViewModel _actions;

    public RemoveFriendDialogView(FriendActionsViewModel actions)
    {
        ArgumentNullException.ThrowIfNull(actions);

        _actions = actions;
        BindingContext = actions;
        Anchor(Gum.Wireframe.Anchor.Center);
        Label title = CreateBoundDialogLabel(nameof(FriendActionsViewModel.DialogTitle), TextStyleCatalog.DialogTitle);
        Label message = CreateBoundDialogLabel(
            nameof(FriendActionsViewModel.DialogMessage),
            TextStyleCatalog.DialogMessage);
        Label keepsHistoryNotice = CreateKeepsHistoryNotice();
        Label error = CreateError();
        StackPanel buttons = CreateButtons();
        Content.AddChild(title);
        Content.AddChild(message);
        Content.AddChild(keepsHistoryNotice);
        Content.AddChild(error);
        Content.AddChild(buttons);
    }

    private static Label CreateBoundDialogLabel(string textProperty, TextStyle style)
    {
        return ViewFactory.CreateBoundUntranslatedLabel(textProperty, style, DialogMetrics.ContentWidth);
    }

    private static Label CreateKeepsHistoryNotice()
    {
        Label keepsHistory = ViewFactory.CreateLabel(nameof(Strings.FriendsRemoveKeepsHistory), TextStyleCatalog.DialogMessage);
        ViewFactory.SetFixedWidth(keepsHistory, DialogMetrics.ContentWidth);
        return keepsHistory;
    }

    private static Label CreateError()
    {
        Label error = CreateBoundDialogLabel(nameof(FriendActionsViewModel.ErrorText), TextStyleCatalog.FieldError);
        error.SetBinding(nameof(IsVisible), nameof(FriendActionsViewModel.HasError));
        return error;
    }

    private StackPanel CreateButtons()
    {
        Button cancel = CreateDialogButton(nameof(Strings.FriendsCancel), ButtonLookCatalog.Secondary);
        cancel.Click += HandleCancelClick;

        Button remove = CreateDialogButton(nameof(Strings.FriendsRemove), ButtonLookCatalog.Primary);
        remove.SetBinding(nameof(IsEnabled), nameof(FriendActionsViewModel.CanConfirm));
        remove.Click += HandleRemoveClick;

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = DialogMetrics.ButtonSpacing,
        };
        row.AddChild(cancel);
        row.AddChild(remove);
        return row;
    }

    private void HandleCancelClick(object? sender, EventArgs e)
    {
        _actions.CloseAll();
    }

    private async void HandleRemoveClick(object? sender, EventArgs e)
    {
        try
        {
            await _actions.ConfirmRemoval();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The friend could not be removed because the connection is closed.", ex);
        }
    }
}
