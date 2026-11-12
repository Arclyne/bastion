using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class RemoveFriendDialogView : DialogCard
{
    private const float ButtonSpacing = 12;
    private const int ButtonsPerRow = 2;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(RemoveFriendDialogView));

    private readonly FriendActionsViewModel _actions;

    public RemoveFriendDialogView(FriendActionsViewModel actions)
    {
        ArgumentNullException.ThrowIfNull(actions);

        _actions = actions;
        BindingContext = actions;
        Anchor(Gum.Wireframe.Anchor.Center);
        Content.AddChild(CreateBoundLabel(nameof(FriendActionsViewModel.DialogTitle), TextStyles.DialogTitle));
        Content.AddChild(CreateBoundLabel(nameof(FriendActionsViewModel.DialogMessage), TextStyles.DialogMessage));
        Content.AddChild(CreateKeepsHistoryNotice());
        Content.AddChild(CreateError());
        Content.AddChild(CreateButtons());
    }

    private static Label CreateBoundLabel(string textProperty, TextStyle style)
    {
        return ViewFactory.CreateBoundUntranslatedLabel(textProperty, style, DialogMetrics.ContentWidth);
    }

    private static Label CreateKeepsHistoryNotice()
    {
        Label keepsHistory = ViewFactory.CreateLabel(nameof(Strings.FriendsRemoveKeepsHistory), TextStyles.DialogMessage);
        keepsHistory.Width = DialogMetrics.ContentWidth;
        keepsHistory.WidthUnits = DimensionUnitType.Absolute;
        return keepsHistory;
    }

    private static Label CreateError()
    {
        Label error = CreateBoundLabel(nameof(FriendActionsViewModel.ErrorText), TextStyles.FieldError);
        error.SetBinding(nameof(IsVisible), nameof(FriendActionsViewModel.HasError));
        return error;
    }

    private StackPanel CreateButtons()
    {
        float buttonWidth = (DialogMetrics.ContentWidth - ButtonSpacing) / ButtonsPerRow;

        Button cancel = ViewFactory.CreateButton(nameof(Strings.FriendsCancelRequest), buttonWidth, DialogMetrics.ButtonHeight);
        ButtonSkin.CreateAndAttach(cancel, ButtonLooks.Secondary);
        cancel.Click += HandleCancelClick;

        Button remove = ViewFactory.CreateButton(nameof(Strings.FriendsRemove), buttonWidth, DialogMetrics.ButtonHeight);
        ButtonSkin.CreateAndAttach(remove, ButtonLooks.Primary);
        remove.SetBinding(nameof(IsEnabled), nameof(FriendActionsViewModel.CanConfirm));
        remove.Click += HandleRemoveClick;

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = ButtonSpacing,
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
