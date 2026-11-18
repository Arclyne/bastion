using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class FriendActionsMenu : Panel
{
    private const float MenuWidth = PanelMetrics.ContentWidth;
    private const float PaddingX = 16;
    private const float PaddingTop = 14;
    private const float PaddingBottom = 16;
    private const float ItemSpacing = 8;
    private const float ItemHeight = 40;
    private const int HorizontalSideCount = 2;
    private const float ItemWidth = MenuWidth - (PaddingX * HorizontalSideCount);

    private readonly FriendActionsViewModel _actions;

    public FriendActionsMenu(FriendActionsViewModel actions)
    {
        ArgumentNullException.ThrowIfNull(actions);

        _actions = actions;
        BindingContext = actions;
        Width = MenuWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Height = PaddingBottom;
        HeightUnits = DimensionUnitType.RelativeToChildren;
        this.SetBinding(nameof(IsVisible), nameof(FriendActionsViewModel.IsMenuOpen));
        StackPanel content = CreateContent();
        AddChild(content);
        CardSkin.Apply(Visual);
    }

    private StackPanel CreateContent()
    {
        Label nickname = ViewFactory.CreateBoundUntranslatedLabel(nameof(FriendActionsViewModel.Nickname), TextStyleCatalog.IdentityName);

        Button viewProfileButton = CreateItem(nameof(Strings.FriendsViewProfile), ButtonLookCatalog.Secondary);
        viewProfileButton.IsEnabled = false;
        Button sendMessageButton = CreateItem(nameof(Strings.FriendsSendMessage), ButtonLookCatalog.Secondary);
        sendMessageButton.Click += HandleSendMessageClick;
        Button removeButton = CreateItem(nameof(Strings.FriendsRemoveFriend), ButtonLookCatalog.Secondary);
        removeButton.Click += HandleRemoveClick;
        Button cancelButton = CreateItem(nameof(Strings.FriendsCancel), ButtonLookCatalog.Pill);
        cancelButton.Click += HandleCancelClick;

        var content = new StackPanel
        {
            X = PaddingX,
            Y = PaddingTop,
            Spacing = ItemSpacing,
        };
        content.AddChild(nickname);
        content.AddChild(viewProfileButton);
        content.AddChild(sendMessageButton);
        content.AddChild(removeButton);
        content.AddChild(cancelButton);
        return content;
    }

    private static Button CreateItem(string textKey, ButtonLook buttonLook)
    {
        Button item = ViewFactory.CreateButton(textKey, ItemWidth, ItemHeight);
        ButtonSkin.CreateAndAttach(item, buttonLook);
        return item;
    }

    private void HandleSendMessageClick(object? sender, EventArgs e)
    {
        _actions.RequestMessage();
    }

    private void HandleRemoveClick(object? sender, EventArgs e)
    {
        _actions.AskRemoval();
    }

    private void HandleCancelClick(object? sender, EventArgs e)
    {
        _actions.CloseAll();
    }
}
