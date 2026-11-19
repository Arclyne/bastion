using System;
using Bastion.Client.Controls;
using Bastion.Client.Localization;
using Bastion.Client.Screens.MainMenu.Chat;
using Bastion.Client.Screens.MainMenu.Friends;
using Bastion.Client.Screens.MainMenu.Rooms;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.GueDeriving;
using log4net;

namespace Bastion.Client.Screens.MainMenu;

public sealed class MainMenuView : ScreenView, IEscapeKeyReceiver
{
    private const float SettingsWidth = 116;
    private const float HowToPlayWidth = 140;
    private const float SettingsOffsetY = (ChipHeight - ScreenMetrics.MenuPillHeight) / VerticalSideCount;
    private const float ChipHeight = 62;
    private const float ChipRadius = ChipHeight / RadiiPerHeight;
    private const int RadiiPerHeight = 2;
    private const float ChipPaddingLeft = 8;
    private const float ChipPaddingRight = 20;
    private const float ChipSpacing = 12;
    private const float ChipShadowOffsetY = 8;
    private const float ChipShadowBlur = 22;
    private const float AvatarSize = 46;
    private const float AvatarRingWidth = 2.5f;
    private const float IdentitySpacing = 3;
    private const float MenuCardWidth = 460;
    private const float MenuCardY = 164;
    private const float MenuCardPaddingX = 30;
    private const float MenuCardPaddingTop = 26;
    private const float MenuCardPaddingBottom = 30;
    private const float MenuCardContentWidth = MenuCardWidth - (MenuCardPaddingX * HorizontalSideCount);
    private const int HorizontalSideCount = 2;
    private const int VerticalSideCount = 2;
    private const float MenuCardSpacing = 20;
    private const float BrandSpacing = 3;
    private const float HeroHeight = 78;
    private const float HeroIconSize = 50;
    private const float HeroIconRadius = 14;
    private const float HeroPaddingX = 14;
    private const float HeroTextX = 78;
    private const float HeroTextSpacing = 3;
    private const float PrivateButtonWidth = (MenuCardContentWidth - PrivateButtonSpacing) / PrivateButtonsPerRow;
    private const float PrivateButtonHeight = 52;
    private const float PrivateButtonSpacing = 14;
    private const int PrivateButtonsPerRow = 2;
    private const float PillSpacing = 10;
    private const float FriendsPillWidth = 112;
    private const float ChatPillWidth = 92;
    private const float FriendsPillRightOffset = ScreenMetrics.MarginX + SettingsWidth + PillSpacing;
    private const float ChatPillRightOffset = FriendsPillRightOffset + FriendsPillWidth + PillSpacing;
    private const float PillRowY = ScreenMetrics.MarginY + SettingsOffsetY;
    private const float FriendsPanelGapY = 12;
    private const float FriendsPanelY = PillRowY + ScreenMetrics.MenuPillHeight + FriendsPanelGapY;
    private const float BadgeOverlap = 8;
    private const float SearchPanelMarginY = 88;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(MainMenuView));

    private readonly FriendsPanelViewModel _friendsPanelViewModel;
    private readonly MatchSearchViewModel _matchSearchViewModel;
    private readonly PrivateRoomViewModel _privateRoom;

    public MainMenuView(
        MainMenuViewModel viewModel,
        FriendsPanelViewModel friendsPanelViewModel,
        MatchSearchViewModel matchSearchViewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(friendsPanelViewModel);
        ArgumentNullException.ThrowIfNull(matchSearchViewModel);

        _friendsPanelViewModel = friendsPanelViewModel;
        _matchSearchViewModel = matchSearchViewModel;
        _privateRoom = viewModel.PrivateRoom;
        BindingContext = viewModel;
        var requestsBinding = new BadgeBinding(
            nameof(FriendRequestsViewModel.PendingCountText),
            nameof(FriendRequestsViewModel.HasPending));
        var unreadBinding = new BadgeBinding(nameof(ChatTabViewModel.UnreadText), nameof(ChatTabViewModel.HasUnread));
        Panel identityChipPanel = CreateIdentityChip();
        Button settingsButton = CreateSettingsButton();
        Button friendsButton = CreateFriendsButton();
        Button chatButton = CreateChatButton();
        Panel requestsBadgePanel = CreateCountBadge(
            friendsPanelViewModel.Requests,
            requestsBinding,
            FriendsPillRightOffset);
        Panel unreadBadgePanel = CreateCountBadge(friendsPanelViewModel.Chat, unreadBinding, ChatPillRightOffset);
        Panel menuCardPanel = CreateMenuCard();
        MatchSearchPanel matchSearchPanel = CreateMatchSearchPanel();
        Button howToPlayButton = CreateHowToPlayButton();
        Label versionLabel = ViewFactory.CreateVersionLabel();
        FriendsPanel friendsPanel = CreateFriendsPanel(friendsPanelViewModel);
        FriendRequestToast friendRequestToast = CreateRequestToast(friendsPanelViewModel.Requests);
        var returningToMatchOverlay = new ReturningToMatchOverlay(viewModel);
        var privateRoomPanel = new PrivateRoomPanel(_privateRoom);
        RoomNoticeStrip roomNoticeStrip = CreateRoomNotice();
        RoomNoticeStrip settingsNoticeStrip = CreateSettingsNotice();
        AddChild(identityChipPanel);
        AddChild(settingsButton);
        AddChild(friendsButton);
        AddChild(chatButton);
        AddChild(requestsBadgePanel);
        AddChild(unreadBadgePanel);
        AddChild(menuCardPanel);
        AddChild(privateRoomPanel);
        AddChild(matchSearchPanel);
        AddChild(howToPlayButton);
        AddChild(versionLabel);
        AddChild(friendsPanel);
        AddChild(friendRequestToast);
        AddChild(roomNoticeStrip);
        AddChild(settingsNoticeStrip);
        AddChild(returningToMatchOverlay);
        var removeFriendDialogView = new RemoveFriendDialogView(friendsPanelViewModel.Actions);
        var presenceDialogView = new PresenceDialogView(matchSearchViewModel);
        AddDialog(removeFriendDialogView, friendsPanelViewModel.Actions, nameof(FriendActionsViewModel.IsDialogOpen));
        AddDialog(presenceDialogView, matchSearchViewModel, nameof(MatchSearchViewModel.IsPresenceDialogOpen));
        AddRoomDialogs();
    }

    public void ReceiveEscapeKey()
    {
        if (_privateRoom.CloseOpenDialog())
        {
            return;
        }

        _friendsPanelViewModel.Close();
    }

    private RoomNoticeStrip CreateRoomNotice()
    {
        var binding = new NoticeBinding(
            nameof(PrivateRoomViewModel.NoticeText),
            nameof(PrivateRoomViewModel.HasNotice));
        return new RoomNoticeStrip(Palette.InfoSoft, binding, HandleRoomNoticeClose)
        {
            BindingContext = _privateRoom,
        };
    }

    private void HandleRoomNoticeClose(object? sender, EventArgs e)
    {
        _privateRoom.DismissNotice();
    }

    private RoomNoticeStrip CreateSettingsNotice()
    {
        var binding = new NoticeBinding(
            nameof(PrivateRoomViewModel.SettingsNoticeText),
            nameof(PrivateRoomViewModel.HasSettingsNotice));
        return new RoomNoticeStrip(Palette.WarningSoft, binding, HandleSettingsNoticeClose)
        {
            BindingContext = _privateRoom,
        };
    }

    private void HandleSettingsNoticeClose(object? sender, EventArgs e)
    {
        _privateRoom.DismissSettingsNotice();
    }

    private void AddRoomDialogs()
    {
        var settingsSheetView = new RoomSettingsSheetView(_privateRoom);
        var joinDialogView = new JoinRoomDialogView(_privateRoom);
        var kickDialogView = new KickPlayerDialogView(_privateRoom);
        AddDialog(settingsSheetView, _privateRoom.Sheet, nameof(RoomSettingsSheetViewModel.IsOpen));
        AddDialog(joinDialogView, _privateRoom.JoinDialog, nameof(JoinRoomDialogViewModel.IsOpen));
        AddDialog(kickDialogView, _privateRoom, nameof(PrivateRoomViewModel.IsKickDialogOpen));
    }

    private static Panel CreateIdentityChip()
    {
        var contentStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = ChipSpacing,
        };
        Panel avatarPanel = ShapeFactory.CreateAvatar(AvatarSize, Palette.IdentityAccent, AvatarRingWidth);
        StackPanel identityTextStackPanel = CreateIdentityText();
        contentStackPanel.AddChild(avatarPanel);
        contentStackPanel.AddChild(identityTextStackPanel);
        contentStackPanel.Anchor(Gum.Wireframe.Anchor.Left);
        contentStackPanel.X = ChipPaddingLeft;

        var chipPanel = new Panel
        {
            X = ScreenMetrics.MarginX,
            Y = ScreenMetrics.MarginY,
            Width = ChipPaddingRight,
            WidthUnits = DimensionUnitType.RelativeToChildren,
            Height = ChipHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        chipPanel.AddChild(contentStackPanel);

        RectangleRuntime surface = ShapeFactory.CreateRaisedSurface(ChipRadius, ChipShadowOffsetY, ChipShadowBlur);
        ShapeFactory.InsertBehindContent(chipPanel.Visual, surface);
        return chipPanel;
    }

    private static StackPanel CreateIdentityText()
    {
        Label nicknameLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(MainMenuViewModel.Nickname),
            TextStyleCatalog.IdentityName);
        Label modeLabel = ViewFactory.CreateLabel(nameof(Strings.MainMenuClassicMode), TextStyleCatalog.IdentityDetail);

        var textColumnStackPanel = new StackPanel
        {
            Spacing = IdentitySpacing,
        };
        textColumnStackPanel.AddChild(nicknameLabel);
        textColumnStackPanel.AddChild(modeLabel);
        ViewFactory.CenterVertically(textColumnStackPanel);
        return textColumnStackPanel;
    }

    private static Button CreateSettingsButton()
    {
        return CreateTopRightPill(nameof(Strings.MainMenuSettings), SettingsWidth, ScreenMetrics.MarginX);
    }

    private static Button CreateTopRightPill(string textKey, float width, float rightOffset)
    {
        Button button = CreatePillButton(textKey, width);
        button.Anchor(Gum.Wireframe.Anchor.TopRight);
        button.X = -rightOffset;
        button.Y = PillRowY;
        return button;
    }

    private static Button CreatePillButton(string textKey, float width)
    {
        Button button = ViewFactory.CreateButton(textKey, width, ScreenMetrics.MenuPillHeight);
        ButtonSkin.CreateAndAttach(button, ButtonLookCatalog.Pill);
        return button;
    }

    private Button CreateFriendsButton()
    {
        Button button = CreateTopRightPill(nameof(Strings.MainMenuFriends), FriendsPillWidth, FriendsPillRightOffset);
        button.Click += HandleFriendsClick;
        return button;
    }

    private void HandleFriendsClick(object? sender, EventArgs e)
    {
        _friendsPanelViewModel.Open(FriendsTab.Friends);
    }

    private Button CreateChatButton()
    {
        Button button = CreateTopRightPill(nameof(Strings.MainMenuChat), ChatPillWidth, ChatPillRightOffset);
        button.Click += HandleChatClick;
        return button;
    }

    private void HandleChatClick(object? sender, EventArgs e)
    {
        _friendsPanelViewModel.Open(FriendsTab.Chat);
    }

    private static Panel CreateCountBadge(object bindingContext, BadgeBinding binding, float pillRightOffset)
    {
        var countBadge = new CountBadge(binding.TextProperty)
        {
            BindingContext = bindingContext,
        };
        countBadge.Anchor(Gum.Wireframe.Anchor.TopRight);
        float badgeX = -pillRightOffset + BadgeOverlap;
        countBadge.X = badgeX;
        countBadge.Y = PillRowY - BadgeOverlap;
        countBadge.SetBinding(nameof(IsVisible), binding.VisibilityProperty);
        return countBadge;
    }

    private Panel CreateMenuCard()
    {
        var contentStackPanel = new StackPanel
        {
            X = MenuCardPaddingX,
            Y = MenuCardPaddingTop,
            Width = MenuCardContentWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Spacing = MenuCardSpacing,
        };
        Panel brandPanel = CreateBrand();
        Button findMatchButton = CreateFindMatchButton();
        Label matchSearchStatusLabel = CreateMatchSearchStatus();
        Label privateMatchLabel = ViewFactory.CreateLabel(
            nameof(Strings.MainMenuPrivateMatch),
            TextStyleCatalog.SectionLabel);
        Panel privateMatchHeadingPanel = CreateCenteredRow(privateMatchLabel);
        StackPanel privateMatchButtonsStackPanel = CreatePrivateMatchButtons();
        contentStackPanel.AddChild(brandPanel);
        contentStackPanel.AddChild(findMatchButton);
        contentStackPanel.AddChild(matchSearchStatusLabel);
        contentStackPanel.AddChild(privateMatchHeadingPanel);
        contentStackPanel.AddChild(privateMatchButtonsStackPanel);

        var cardPanel = new Panel
        {
            Width = MenuCardWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = MenuCardPaddingBottom,
            HeightUnits = DimensionUnitType.RelativeToChildren,
        };
        cardPanel.Anchor(Gum.Wireframe.Anchor.Top);
        cardPanel.Y = MenuCardY;
        cardPanel.BindingContext = _privateRoom;
        cardPanel.SetBinding(nameof(IsVisible), nameof(PrivateRoomViewModel.IsMenuShown));
        cardPanel.AddChild(contentStackPanel);
        CardSkin.Apply(cardPanel.Visual);
        return cardPanel;
    }

    private static Panel CreateBrand()
    {
        var brandStackPanel = new StackPanel
        {
            Spacing = BrandSpacing,
        };
        Label appNameLabel = ViewFactory.CreateLabel(nameof(Strings.AppName), TextStyleCatalog.MenuTitle);
        Label taglineLabel = ViewFactory.CreateLabel(nameof(Strings.LoginTagline), TextStyleCatalog.MenuTagline);
        brandStackPanel.AddChild(appNameLabel);
        brandStackPanel.AddChild(taglineLabel);
        return CreateCenteredRow(brandStackPanel);
    }

    private static Panel CreateCenteredRow(FrameworkElement element)
    {
        ViewFactory.CenterHorizontally(element);

        var rowPanel = new Panel
        {
            Width = MenuCardContentWidth,
            WidthUnits = DimensionUnitType.Absolute,
            HeightUnits = DimensionUnitType.RelativeToChildren,
        };
        rowPanel.AddChild(element);
        return rowPanel;
    }

    private Button CreateFindMatchButton()
    {
        Button button = ViewFactory.CreateButton(string.Empty, MenuCardContentWidth, HeroHeight);
        ButtonSkin.CreateAndAttach(button, ButtonLookCatalog.Hero);
        button.BindingContext = _matchSearchViewModel;
        button.SetBinding(nameof(IsEnabled), nameof(MatchSearchViewModel.CanStartSearch));
        button.Click += HandleFindMatchClick;

        button.Visual.ExposeChildrenEvents = false;
        RectangleRuntime icon = ShapeFactory.CreateRounded(Palette.HeroIconFill, HeroIconRadius);
        icon.Anchor(Gum.Wireframe.Anchor.Left);
        icon.X = HeroPaddingX;
        icon.Width = HeroIconSize;
        icon.WidthUnits = DimensionUnitType.Absolute;
        icon.Height = HeroIconSize;
        icon.HeightUnits = DimensionUnitType.Absolute;
        button.AddChild(icon);

        var textColumnStackPanel = new StackPanel
        {
            X = HeroTextX,
            Spacing = HeroTextSpacing,
        };
        Label titleLabel = ViewFactory.CreateLabel(nameof(Strings.MainMenuFindMatch), TextStyleCatalog.HeroTitle);
        Label detailLabel = ViewFactory.CreateLabel(
            nameof(Strings.MainMenuFindMatchDetail),
            TextStyleCatalog.HeroDetail);
        textColumnStackPanel.AddChild(titleLabel);
        textColumnStackPanel.AddChild(detailLabel);
        ViewFactory.CenterVertically(textColumnStackPanel);
        button.AddChild(textColumnStackPanel);
        return button;
    }

    private async void HandleFindMatchClick(object? sender, EventArgs e)
    {
        try
        {
            await _matchSearchViewModel.StartSearch();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The match search could not be started because the connection is closed.", ex);
        }
    }

    private Label CreateMatchSearchStatus()
    {
        Label statusLabel = ViewFactory.CreateBoundLabel(
            nameof(MatchSearchViewModel.StatusKey),
            TextStyleCatalog.FieldError,
            MenuCardContentWidth);
        statusLabel.BindingContext = _matchSearchViewModel;
        statusLabel.SetBinding(nameof(IsVisible), nameof(MatchSearchViewModel.HasStatus));
        return statusLabel;
    }

    private StackPanel CreatePrivateMatchButtons()
    {
        var rowStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = PrivateButtonSpacing,
        };
        Button createRoomButton = CreatePrivateMatchButton(nameof(Strings.MainMenuCreateRoom));
        createRoomButton.Click += HandleCreateRoomClick;
        Button joinWithCodeButton = CreatePrivateMatchButton(nameof(Strings.MainMenuJoinWithCode));
        joinWithCodeButton.Click += HandleJoinWithCodeClick;
        rowStackPanel.AddChild(createRoomButton);
        rowStackPanel.AddChild(joinWithCodeButton);
        return rowStackPanel;
    }

    private Button CreatePrivateMatchButton(string textKey)
    {
        Button button = ViewFactory.CreateButton(textKey, PrivateButtonWidth, PrivateButtonHeight);
        ButtonSkin.CreateAndAttach(button, ButtonLookCatalog.Secondary);
        button.BindingContext = _matchSearchViewModel;
        button.SetBinding(nameof(IsEnabled), nameof(MatchSearchViewModel.CanStartSearch));
        return button;
    }

    private void HandleCreateRoomClick(object? sender, EventArgs e)
    {
        _privateRoom.OpenCreateSheet();
    }

    private void HandleJoinWithCodeClick(object? sender, EventArgs e)
    {
        _privateRoom.OpenJoinDialog();
    }

    private MatchSearchPanel CreateMatchSearchPanel()
    {
        var matchSearchPanel = new MatchSearchPanel(_matchSearchViewModel);
        matchSearchPanel.Anchor(Gum.Wireframe.Anchor.Bottom);
        matchSearchPanel.Y = -SearchPanelMarginY;
        return matchSearchPanel;
    }

    private static Button CreateHowToPlayButton()
    {
        Button button = CreatePillButton(nameof(Strings.MainMenuHowToPlay), HowToPlayWidth);
        button.Anchor(Gum.Wireframe.Anchor.BottomLeft);
        button.X = ScreenMetrics.MarginX;
        button.Y = -ScreenMetrics.MarginY;
        return button;
    }

    private static FriendsPanel CreateFriendsPanel(FriendsPanelViewModel friendsPanelViewModel)
    {
        var friendsPanel = new FriendsPanel(friendsPanelViewModel);
        friendsPanel.Anchor(Gum.Wireframe.Anchor.TopRight);
        friendsPanel.X = -ScreenMetrics.MarginX;
        friendsPanel.Y = FriendsPanelY;
        return friendsPanel;
    }

    private static FriendRequestToast CreateRequestToast(FriendRequestsViewModel requests)
    {
        var friendRequestToast = new FriendRequestToast(requests);
        friendRequestToast.Anchor(Gum.Wireframe.Anchor.Top);
        friendRequestToast.Y = ScreenMetrics.MarginY;
        return friendRequestToast;
    }
}
