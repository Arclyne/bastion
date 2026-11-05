using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
using Gum.GueDeriving;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class FriendsPanel : Panel
{
    private const float PanelWidth = 400;
    private const float PanelHeight = 652;
    private const float PaddingX = 20;
    private const float PaddingTop = 18;
    private const float PaddingBottom = 18;
    private const float ContentWidth = 360;
    private const float ContentSpacing = 12;
    private const float TabWidth = 104;
    private const float TabHeight = 36;
    private const float TabSpacing = 4;
    private const float TabMarkerHeight = 3;
    private const float TabMarkerRadius = 1.5f;
    private const float CloseButtonSize = 36;

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
        content.AddChild(CreatePlaceholder());
        AddChild(content);
        AddChild(CreateFooter(viewModel));
        Skins.ApplyCard(Visual);
    }

    private static Label CreateHeader()
    {
        Label header = ViewFactory.CreateLabel(string.Empty, TextStyles.SectionLabel);
        ((LabelVisual)header.Visual).LocalizeText = false;
        header.SetBinding(nameof(Label.Text), nameof(FriendsPanelViewModel.HeaderText));
        header.SetBinding(nameof(IsVisible), nameof(FriendsPanelViewModel.IsFriendListVisible));
        return header;
    }

    private static FriendListView CreateFriendList(FriendsPanelViewModel viewModel)
    {
        var friendList = new FriendListView(viewModel);
        friendList.SetBinding(nameof(IsVisible), nameof(FriendsPanelViewModel.IsFriendListVisible));
        return friendList;
    }

    private static Panel CreateAddFriend(FriendsPanelViewModel viewModel)
    {
        var container = new Panel
        {
            WidthUnits = DimensionUnitType.RelativeToChildren,
            HeightUnits = DimensionUnitType.RelativeToChildren,
        };
        container.AddChild(new AddFriendView(viewModel));
        container.SetBinding(nameof(IsVisible), nameof(FriendsPanelViewModel.IsAddFriendVisible));
        return container;
    }

    private static Label CreatePlaceholder()
    {
        Label placeholder = ViewFactory.CreateLabel(nameof(Strings.FriendsComingSoon), TextStyles.NoticeTitle);
        placeholder.SetBinding(nameof(IsVisible), nameof(FriendsPanelViewModel.IsPlaceholderTab));
        return placeholder;
    }

    private static FriendCodeFooter CreateFooter(FriendsPanelViewModel viewModel)
    {
        var footer = new FriendCodeFooter(viewModel);
        footer.Anchor(Gum.Wireframe.Anchor.BottomLeft);
        footer.X = PaddingX;
        footer.Y = -PaddingBottom;
        return footer;
    }

    private static StackPanel CreateTab(string textKey, string selectedProperty, EventHandler handleClick)
    {
        Button button = ViewFactory.CreateButton(textKey, TabWidth, TabHeight);
        ButtonSkin.CreateAndAttach(button, ButtonLooks.Pill);
        button.Click += handleClick;

        RectangleRuntime marker = Shapes.CreateRounded(Palette.Accent, TabMarkerRadius);
        marker.Width = TabWidth;
        marker.WidthUnits = DimensionUnitType.Absolute;
        marker.Height = TabMarkerHeight;
        marker.HeightUnits = DimensionUnitType.Absolute;
        var markerHolder = new Panel
        {
            Width = TabWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = TabMarkerHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        markerHolder.AddChild(marker);
        markerHolder.SetBinding(nameof(IsVisible), selectedProperty);

        var tab = new StackPanel { Spacing = TabSpacing };
        tab.AddChild(button);
        tab.AddChild(markerHolder);
        return tab;
    }

    private StackPanel CreateTabRow()
    {
        Button close = ViewFactory.CreateButton(nameof(Strings.FriendsCloseButton), CloseButtonSize, CloseButtonSize);
        ButtonSkin.CreateAndAttach(close, ButtonLooks.Pill);
        close.Click += HandleCloseClick;

        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = TabSpacing };
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
