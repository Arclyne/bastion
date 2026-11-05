using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class RemoveFriendDialogView : Panel
{
    private const float DialogWidth = 420;
    private const float PaddingX = 28;
    private const float PaddingTop = 26;
    private const float PaddingBottom = 24;
    private const float SectionSpacing = 14;
    private const float ButtonSpacing = 12;
    private const float ButtonHeight = 46;
    private const int HorizontalSideCount = 2;
    private const int ButtonsPerRow = 2;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(RemoveFriendDialogView));

    private readonly FriendActionsViewModel _actions;

    public RemoveFriendDialogView(FriendActionsViewModel actions)
    {
        ArgumentNullException.ThrowIfNull(actions);

        _actions = actions;
        BindingContext = actions;
        Anchor(Gum.Wireframe.Anchor.Center);
        Width = DialogWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Height = PaddingBottom;
        HeightUnits = DimensionUnitType.RelativeToChildren;
        AddChild(CreateContent());
        Skins.ApplyCard(Visual);
    }

    private static float ContentWidth => DialogWidth - (PaddingX * HorizontalSideCount);

    private static Label CreateBoundLabel(string textProperty, TextStyle style)
    {
        var label = new Label { Width = ContentWidth, WidthUnits = DimensionUnitType.Absolute };
        style.ApplyTo((LabelVisual)label.Visual);
        ((LabelVisual)label.Visual).LocalizeText = false;
        label.SetBinding(nameof(Label.Text), textProperty);
        return label;
    }

    private StackPanel CreateContent()
    {
        var keepsHistory = new Label { Width = ContentWidth, WidthUnits = DimensionUnitType.Absolute };
        TextStyles.DialogMessage.ApplyTo((LabelVisual)keepsHistory.Visual);
        keepsHistory.Text = nameof(Strings.FriendsRemoveKeepsHistory);

        Label error = CreateBoundLabel(nameof(FriendActionsViewModel.ErrorText), TextStyles.FieldError);
        error.SetBinding(nameof(IsVisible), nameof(FriendActionsViewModel.HasError));

        var content = new StackPanel { X = PaddingX, Y = PaddingTop, Spacing = SectionSpacing };
        content.AddChild(CreateBoundLabel(nameof(FriendActionsViewModel.DialogTitle), TextStyles.DialogTitle));
        content.AddChild(CreateBoundLabel(nameof(FriendActionsViewModel.DialogMessage), TextStyles.DialogMessage));
        content.AddChild(keepsHistory);
        content.AddChild(error);
        content.AddChild(CreateButtons());
        return content;
    }

    private StackPanel CreateButtons()
    {
        float buttonWidth = (ContentWidth - ButtonSpacing) / ButtonsPerRow;

        Button cancel = ViewFactory.CreateButton(nameof(Strings.FriendsCancelRequest), buttonWidth, ButtonHeight);
        ButtonSkin.CreateAndAttach(cancel, ButtonLooks.Secondary);
        cancel.Click += HandleCancelClick;

        Button remove = ViewFactory.CreateButton(nameof(Strings.FriendsRemove), buttonWidth, ButtonHeight);
        ButtonSkin.CreateAndAttach(remove, ButtonLooks.Primary);
        remove.SetBinding(nameof(IsEnabled), nameof(FriendActionsViewModel.CanConfirm));
        remove.Click += HandleRemoveClick;

        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = ButtonSpacing };
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
