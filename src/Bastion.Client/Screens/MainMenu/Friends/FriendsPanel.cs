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
    private const float PanelWidth = ContentWidth + (PaddingX * HorizontalSideCount);
    private const float PanelHeight = 652;
    private const float PaddingX = 20;
    private const float PaddingTop = 18;
    private const float PaddingBottom = 18;
    private const float ContentWidth = PanelMetrics.ContentWidth;
    private const float ContentSpacing = 12;
    private const float TabWidth = (ContentWidth - CloseButtonSize - (TabSpacing * TabCount)) / TabCount;
    private const float TabHeight = 36;
    private const float TabSpacing = 4;
    private const float CloseButtonSize = 36;
    private const int TabCount = 3;
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

        var contentStackPanel = new StackPanel
        {
            X = PaddingX,
            Y = PaddingTop,
            Width = ContentWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Spacing = ContentSpacing,
        };
        StackPanel tabRowStackPanel = CreateTabRow();
        Label headerLabel = CreateHeader();
        FriendListView friendListView = CreateFriendList(viewModel);
        Panel addFriendPanel = CreateAddFriend(viewModel);
        Panel requestsPanel = CreateRequests(viewModel);
        Panel chatPanel = CreateChat(viewModel);
        contentStackPanel.AddChild(tabRowStackPanel);
        contentStackPanel.AddChild(headerLabel);
        contentStackPanel.AddChild(friendListView);
        contentStackPanel.AddChild(addFriendPanel);
        contentStackPanel.AddChild(requestsPanel);
        contentStackPanel.AddChild(chatPanel);
        FriendCodeFooter friendCodeFooter = CreateFooter(viewModel);
        FriendActionsMenu friendActionsMenu = CreateActionsMenu(viewModel);
        AddChild(contentStackPanel);
        AddChild(friendCodeFooter);
        AddChild(friendActionsMenu);
        CardSkin.Apply(Visual);
    }

    private StackPanel CreateTabRow()
    {
        Button closeButton = ViewFactory.CreateButton(
            nameof(Strings.FriendsCloseButton),
            CloseButtonSize,
            CloseButtonSize);
        ButtonSkin.CreateAndAttach(closeButton, ButtonLookCatalog.Pill);
        closeButton.Click += HandleCloseClick;

        var rowStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = TabSpacing,
        };
        TabButton friendsTabButton = CreateTab(
            nameof(Strings.FriendsTabFriends),
            nameof(FriendsPanelViewModel.IsFriendsTab),
            HandleFriendsTabClick);
        TabButton requestsTabButton = CreateTab(
            nameof(Strings.FriendsTabRequests),
            nameof(FriendsPanelViewModel.IsRequestsTab),
            HandleRequestsTabClick);
        TabButton chatTabButton = CreateTab(
            nameof(Strings.FriendsTabChat),
            nameof(FriendsPanelViewModel.IsChatTab),
            HandleChatTabClick);
        rowStackPanel.AddChild(friendsTabButton);
        rowStackPanel.AddChild(requestsTabButton);
        rowStackPanel.AddChild(chatTabButton);
        rowStackPanel.AddChild(closeButton);
        return rowStackPanel;
    }

    private void HandleCloseClick(object? sender, EventArgs e)
    {
        _viewModel.Close();
    }

    private static TabButton CreateTab(string textKey, string selectedProperty, EventHandler handleClick)
    {
        Button button = ViewFactory.CreateButton(textKey, TabWidth, TabHeight);
        ButtonSkin.CreateAndAttach(button, ButtonLookCatalog.Pill);
        button.Click += handleClick;
        return new TabButton(button, selectedProperty, TabSpacing);
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

    private static Label CreateHeader()
    {
        return ViewFactory.CreateBoundNotice(
            nameof(FriendsPanelViewModel.HeaderText),
            nameof(FriendsPanelViewModel.IsFriendListVisible),
            TextStyleCatalog.SectionLabel);
    }

    private static FriendListView CreateFriendList(FriendsPanelViewModel viewModel)
    {
        var friendListView = new FriendListView(viewModel);
        friendListView.SetBinding(nameof(IsVisible), nameof(FriendsPanelViewModel.IsFriendListVisible));
        return friendListView;
    }

    private static Panel CreateAddFriend(FriendsPanelViewModel viewModel)
    {
        var addFriendView = new AddFriendView(viewModel);
        return ViewFactory.CreateToggledContainer(addFriendView, nameof(FriendsPanelViewModel.IsAddFriendVisible));
    }

    private static Panel CreateRequests(FriendsPanelViewModel viewModel)
    {
        var friendRequestsView = new FriendRequestsView(viewModel.Requests);
        return ViewFactory.CreateToggledContainer(friendRequestsView, nameof(FriendsPanelViewModel.IsRequestsTab));
    }

    private static Panel CreateChat(FriendsPanelViewModel viewModel)
    {
        var chatTabView = new ChatTabView(viewModel.Chat);
        return ViewFactory.CreateToggledContainer(chatTabView, nameof(FriendsPanelViewModel.IsChatTab));
    }

    private static FriendCodeFooter CreateFooter(FriendsPanelViewModel viewModel)
    {
        var friendCodeFooter = new FriendCodeFooter(viewModel);
        PlaceAtBottomLeft(friendCodeFooter);
        friendCodeFooter.SetBinding(nameof(IsVisible), nameof(FriendsPanelViewModel.IsFriendCodeVisible));
        return friendCodeFooter;
    }

    private static void PlaceAtBottomLeft(FrameworkElement element)
    {
        element.Anchor(Gum.Wireframe.Anchor.BottomLeft);
        element.X = PaddingX;
        element.Y = -PaddingBottom;
    }

    private static FriendActionsMenu CreateActionsMenu(FriendsPanelViewModel viewModel)
    {
        var friendActionsMenu = new FriendActionsMenu(viewModel.Actions);
        PlaceAtBottomLeft(friendActionsMenu);
        return friendActionsMenu;
    }
}
