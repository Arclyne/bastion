using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class FriendActionsMenu : Panel
{
    private const float MenuWidth = 360;
    private const float PaddingX = 16;
    private const float PaddingTop = 14;
    private const float PaddingBottom = 16;
    private const float ItemSpacing = 8;
    private const float ItemHeight = 40;
    private const int HorizontalSideCount = 2;

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
        AddChild(CreateContent());
        Skins.ApplyCard(Visual);
    }

    private static Button CreateItem(string textKey, ButtonLook look)
    {
        Button item = ViewFactory.CreateButton(textKey, MenuWidth - (PaddingX * HorizontalSideCount), ItemHeight);
        ButtonSkin.CreateAndAttach(item, look);
        return item;
    }

    private StackPanel CreateContent()
    {
        Label nickname = ViewFactory.CreateLabel(string.Empty, TextStyles.IdentityName);
        ((LabelVisual)nickname.Visual).LocalizeText = false;
        nickname.SetBinding(nameof(Label.Text), nameof(FriendActionsViewModel.Nickname));

        Button viewProfile = CreateItem(nameof(Strings.FriendsViewProfile), ButtonLooks.Secondary);
        viewProfile.IsEnabled = false;
        Button remove = CreateItem(nameof(Strings.FriendsRemoveFriend), ButtonLooks.Secondary);
        remove.Click += HandleRemoveClick;
        Button cancel = CreateItem(nameof(Strings.FriendsCancelRequest), ButtonLooks.Pill);
        cancel.Click += HandleCancelClick;

        var content = new StackPanel { X = PaddingX, Y = PaddingTop, Spacing = ItemSpacing };
        content.AddChild(nickname);
        content.AddChild(viewProfile);
        content.AddChild(remove);
        content.AddChild(cancel);
        return content;
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
