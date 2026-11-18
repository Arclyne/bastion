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
        Label titleLabel = CreateBoundDialogLabel(
            nameof(FriendActionsViewModel.DialogTitle),
            TextStyleCatalog.DialogTitle);
        Label messageLabel = CreateBoundDialogLabel(
            nameof(FriendActionsViewModel.DialogMessage),
            TextStyleCatalog.DialogMessage);
        Label keepsHistoryNoticeLabel = CreateKeepsHistoryNotice();
        Label errorLabel = CreateError();
        StackPanel buttonsStackPanel = CreateButtons();
        Content.AddChild(titleLabel);
        Content.AddChild(messageLabel);
        Content.AddChild(keepsHistoryNoticeLabel);
        Content.AddChild(errorLabel);
        Content.AddChild(buttonsStackPanel);
    }

    private static Label CreateBoundDialogLabel(string textProperty, TextStyle style)
    {
        return ViewFactory.CreateBoundUntranslatedLabel(textProperty, style, DialogMetrics.ContentWidth);
    }

    private static Label CreateKeepsHistoryNotice()
    {
        Label keepsHistoryLabel = ViewFactory.CreateLabel(
            nameof(Strings.FriendsRemoveKeepsHistory),
            TextStyleCatalog.DialogMessage);
        ViewFactory.SetFixedWidth(keepsHistoryLabel, DialogMetrics.ContentWidth);
        return keepsHistoryLabel;
    }

    private static Label CreateError()
    {
        Label errorLabel = CreateBoundDialogLabel(
            nameof(FriendActionsViewModel.ErrorText),
            TextStyleCatalog.FieldError);
        errorLabel.SetBinding(nameof(IsVisible), nameof(FriendActionsViewModel.HasError));
        return errorLabel;
    }

    private StackPanel CreateButtons()
    {
        Button cancelButton = CreateDialogButton(nameof(Strings.FriendsCancel), ButtonLookCatalog.Secondary);
        cancelButton.Click += HandleCancelClick;

        Button removeButton = CreateDialogButton(nameof(Strings.FriendsRemove), ButtonLookCatalog.Primary);
        removeButton.SetBinding(nameof(IsEnabled), nameof(FriendActionsViewModel.CanConfirm));
        removeButton.Click += HandleRemoveClick;

        var rowStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = DialogMetrics.ButtonSpacing,
        };
        rowStackPanel.AddChild(cancelButton);
        rowStackPanel.AddChild(removeButton);
        return rowStackPanel;
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
