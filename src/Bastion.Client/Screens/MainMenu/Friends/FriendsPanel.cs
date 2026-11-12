using System;
using Bastion.Client.Controls;
using Bastion.Client.Localization;
using Bastion.Client.Screens.MainMenu.Chat;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class FriendsPanel : Panel
{
    private const float PanelWidth = PanelMetrics.ContentWidth + (PaddingX * HorizontalSideCount);
    private const float PanelHeight = 652;
    private const float PaddingX = 20;
    private const float PaddingTop = 18;
    private const float PaddingBottom = 18;
    private const float ContentWidth = PanelMetrics.ContentWidth;
    private const float ContentSpacing = 12;
    private const float TabWidth = 104;
    private const float TabHeight = 36;
    private const float TabSpacing = 4;
    private const float CloseButtonSize = 36;
    private const int HorizontalSideCount = 2;

    private readonly FriendsPanelViewModel _viewModel;

    public FriendsPanel(FriendsPanelViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;
        BindingContext = viewModel;
        Width = PanelWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Height = PanelHeight;
        HeightUnits = DimensionUnitType.Absolute;
        this.SetBinding(nameof(IsVisible), nameof(FriendsPanelViewModel.IsOpen));

        var content = new StackPanel
        {
            X = PaddingX,
            Y = PaddingTop,
            Width = ContentWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Spacing = ContentSpacing,
        };
        content.AddChild(CreateTabRow());
        content.AddChild(CreateHeader());
        content.AddChild(CreateFriendList(viewModel));
        content.AddChild(CreateAddFriend(viewModel));
        content.AddChild(CreateRequests(viewModel));
        content.AddChild(CreateChat(viewModel));
        AddChild(content);
        AddChild(CreateFooter(viewModel));
        AddChild(CreateActionsMenu(viewModel));
        CardSkin.Apply(Visual);
    }

    private StackPanel CreateTabRow()
    {
        Button close = ViewFactory.CreateButton(nameof(Strings.FriendsCloseButton), CloseButtonSize, CloseButtonSize);
        ButtonSkin.CreateAndAttach(close, ButtonLookCatalog.Pill);
        close.Click += HandleCloseClick;

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = TabSpacing,
        };
        row.AddChild(CreateTab(
            nameof(Strings.FriendsTabFriends),
            nameof(FriendsPanelViewModel.IsFriendsTab),
            HandleFriendsTabClick));
        row.AddChild(CreateTab(
            nameof(Strings.FriendsTabRequests),
            nameof(FriendsPanelViewModel.IsRequestsTab),
            HandleRequestsTabClick));
        row.AddChild(CreateTab(
            nameof(Strings.FriendsTabChat),
            nameof(FriendsPanelViewModel.IsChatTab),
            HandleChatTabClick));
        row.AddChild(close);
        return row;
    }

    private static TabButton CreateTab(string textKey, string selectedProperty, EventHandler handleClick)
    {
        Button button = ViewFactory.CreateButton(textKey, TabWidth, TabHeight);
        ButtonSkin.CreateAndAttach(button, ButtonLookCatalog.Pill);
        button.Click += handleClick;
        return new TabButton(button, selectedProperty, TabSpacing);
    }

    private static Label CreateHeader()
    {
        return ViewFactory.CreateBoundNotice(
            nameof(FriendsPanelViewModel.HeaderText),
            nameof(FriendsPanelViewModel.IsFriendListVisible),
            TextStyleCatalog.SectionLabel);
    }

    private static FriendListView CreateFriendList(FriendsPanelViewModel viewModel)
    {
        var friendList = new FriendListView(viewModel);
        friendList.SetBinding(nameof(IsVisible), nameof(FriendsPanelViewModel.IsFriendListVisible));
        return friendList;
    }

    private static Panel CreateAddFriend(FriendsPanelViewModel viewModel)
    {
        var addFriend = new AddFriendView(viewModel);
        return CreateToggledContainer(addFriend, nameof(FriendsPanelViewModel.IsAddFriendVisible));
    }

    private static Panel CreateToggledContainer(FrameworkElement content, string visibilityProperty)
    {
        var container = new Panel
        {
            WidthUnits = DimensionUnitType.RelativeToChildren,
            HeightUnits = DimensionUnitType.RelativeToChildren,
        };
        container.AddChild(content);
        container.SetBinding(nameof(IsVisible), visibilityProperty);
        return container;
    }

    private static Panel CreateRequests(FriendsPanelViewModel viewModel)
    {
        var requests = new FriendRequestsView(viewModel.Requests);
        return CreateToggledContainer(requests, nameof(FriendsPanelViewModel.IsRequestsTab));
    }

    private static Panel CreateChat(FriendsPanelViewModel viewModel)
    {
        var chatTab = new ChatTabView(viewModel.Chat);
        return CreateToggledContainer(chatTab, nameof(FriendsPanelViewModel.IsChatTab));
    }

    private static FriendCodeFooter CreateFooter(FriendsPanelViewModel viewModel)
    {
        var footer = new FriendCodeFooter(viewModel);
        footer.Anchor(Gum.Wireframe.Anchor.BottomLeft);
        footer.X = PaddingX;
        footer.Y = -PaddingBottom;
        footer.SetBinding(nameof(IsVisible), nameof(FriendsPanelViewModel.ShowsFriendCode));
        return footer;
    }

    private static FriendActionsMenu CreateActionsMenu(FriendsPanelViewModel viewModel)
    {
        var menu = new FriendActionsMenu(viewModel.Actions);
        menu.Anchor(Gum.Wireframe.Anchor.BottomLeft);
        menu.X = PaddingX;
        menu.Y = -PaddingBottom;
        return menu;
    }

    private void HandleFriendsTabClick(object? sender, EventArgs e)
    {
        _viewModel.SelectTab(FriendsTab.Friends);
    }

    private void HandleRequestsTabClick(object? sender, EventArgs e)
    {
        _viewModel.SelectTab(FriendsTab.Requests);
    }

    private void HandleChatTabClick(object? sender, EventArgs e)
    {
        _viewModel.SelectTab(FriendsTab.Chat);
    }

    private void HandleCloseClick(object? sender, EventArgs e)
    {
        _viewModel.Close();
    }
}
