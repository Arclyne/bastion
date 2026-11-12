using System;
using Bastion.Client.Localization;
using Bastion.Client.Screens.MainMenu.Chat;
using Bastion.Client.Screens.MainMenu.Friends;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.GueDeriving;
using Gum.Wireframe;
using log4net;

namespace Bastion.Client.Screens.MainMenu;

public sealed class MainMenuView : ScreenView, IEscapeKeyReceiver
{
    private const float SettingsWidth = 116;
    private const float HowToPlayWidth = 140;
    private const float SettingsOffsetY = 9;
    private const float ChipHeight = 62;
    private const float ChipRadius = 31;
    private const float ChipPaddingLeft = 8;
    private const float ChipPaddingRight = 20;
    private const float ChipSpacing = 12;
    private const float ChipBorderWidth = 1.5f;
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
    private const float MenuCardSpacing = 20;
    private const float BrandSpacing = 3;
    private const float HeroHeight = 78;
    private const float HeroIconSize = 50;
    private const float HeroIconRadius = 14;
    private const float HeroPaddingX = 14;
    private const float HeroTextX = 78;
    private const float HeroTextSpacing = 3;
    private const float PrivateButtonWidth = 193;
    private const float PrivateButtonHeight = 52;
    private const float PrivateButtonSpacing = 14;
    private const float PillSpacing = 10;
    private const float FriendsPillWidth = 112;
    private const float ChatPillWidth = 92;
    private const float FriendsPillRightOffset = ScreenMetrics.MarginX + SettingsWidth + PillSpacing;
    private const float ChatPillRightOffset = FriendsPillRightOffset + FriendsPillWidth + PillSpacing;
    private const float PillRowY = ScreenMetrics.MarginY + SettingsOffsetY;
    private const float FriendsPanelGapY = 12;
    private const float BadgeSize = 22;
    private const float BadgeOverlap = 8;
    private const float SearchPanelMarginY = 88;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(MainMenuView));

    private readonly FriendsPanelViewModel _friendsPanelViewModel;
    private readonly MatchSearchViewModel _matchSearchViewModel;

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
        BindingContext = viewModel;
        AddChild(CreateIdentityChip());
        AddChild(CreateSettingsButton());
        AddChild(CreateFriendsButton());
        AddChild(CreateChatButton());
        AddChild(CreateCountBadge(
            friendsPanelViewModel.Requests,
            new BadgeBinding(
                nameof(FriendRequestsViewModel.PendingCountText),
                nameof(FriendRequestsViewModel.HasPending)),
            FriendsPillRightOffset));
        AddChild(CreateCountBadge(
            friendsPanelViewModel.Chat,
            new BadgeBinding(nameof(ChatTabViewModel.UnreadText), nameof(ChatTabViewModel.HasUnread)),
            ChatPillRightOffset));
        AddChild(CreateMenuCard());
        AddChild(CreateMatchSearchPanel());
        AddChild(CreateHowToPlayButton());
        AddChild(ViewFactory.CreateVersionLabel());
        AddChild(CreateFriendsPanel(friendsPanelViewModel));
        AddChild(CreateRequestToast(friendsPanelViewModel.Requests));
        AddDialog(
            new RemoveFriendDialogView(friendsPanelViewModel.Actions),
            friendsPanelViewModel.Actions,
            nameof(FriendActionsViewModel.IsDialogOpen));
        AddDialog(
            new PresenceDialogView(matchSearchViewModel),
            matchSearchViewModel,
            nameof(MatchSearchViewModel.IsPresenceDialogOpen));
    }

    public void ReceiveEscapeKey()
    {
        _friendsPanelViewModel.Close();
    }

    private static Panel CreateIdentityChip()
    {
        var content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = ChipSpacing,
        };
        content.AddChild(CreateAvatar());
        content.AddChild(CreateIdentityText());
        content.Anchor(Gum.Wireframe.Anchor.Left);
        content.X = ChipPaddingLeft;

        var chip = new Panel
        {
            X = ScreenMetrics.MarginX,
            Y = ScreenMetrics.MarginY,
            Width = ChipPaddingRight,
            WidthUnits = DimensionUnitType.RelativeToChildren,
            Height = ChipHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        chip.AddChild(content);

        RectangleRuntime surface = Shapes.CreateRounded(Palette.Card, ChipRadius);
        surface.StrokeColor = Palette.Border;
        surface.StrokeWidth = ChipBorderWidth;
        surface.HasDropshadow = true;
        surface.DropshadowColor = Palette.Shadow;
        surface.DropshadowOffsetY = ChipShadowOffsetY;
        surface.DropshadowBlur = ChipShadowBlur;
        Shapes.InsertBehindContent(chip.Visual, surface);
        return chip;
    }

    private static Panel CreateAvatar()
    {
        var avatar = new Panel
        {
            Width = AvatarSize,
            WidthUnits = DimensionUnitType.Absolute,
            Height = AvatarSize,
            HeightUnits = DimensionUnitType.Absolute,
        };
        RectangleRuntime circle = Shapes.CreateCircle(Palette.AvatarFill, AvatarSize);
        circle.StrokeColor = Palette.PlayerOne;
        circle.StrokeWidth = AvatarRingWidth;
        Shapes.InsertBehindContent(avatar.Visual, circle);
        return avatar;
    }

    private static StackPanel CreateIdentityText()
    {
        Label nickname = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(MainMenuViewModel.Nickname),
            TextStyles.IdentityName);

        var text = new StackPanel
        {
            Spacing = IdentitySpacing,
        };
        text.AddChild(nickname);
        text.AddChild(ViewFactory.CreateLabel(nameof(Strings.MainMenuClassicMode), TextStyles.IdentityDetail));
        ViewFactory.CenterVertically(text);
        return text;
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
        ButtonSkin.CreateAndAttach(button, ButtonLooks.Pill);
        return button;
    }

    private Button CreateFriendsButton()
    {
        Button button = CreateTopRightPill(nameof(Strings.MainMenuFriends), FriendsPillWidth, FriendsPillRightOffset);
        button.Click += HandleFriendsClick;
        return button;
    }

    private Button CreateChatButton()
    {
        Button button = CreateTopRightPill(nameof(Strings.MainMenuChat), ChatPillWidth, ChatPillRightOffset);
        button.Click += HandleChatClick;
        return button;
    }

    private static Panel CreateCountBadge(object bindingContext, BadgeBinding binding, float pillRightOffset)
    {
        Label count = ViewFactory.CreateBoundUntranslatedLabel(binding.TextProperty, TextStyles.PrimaryButton);
        ViewFactory.CenterHorizontally(count);
        ViewFactory.CenterVertically(count);

        var badge = new Panel
        {
            Width = BadgeSize,
            WidthUnits = DimensionUnitType.Absolute,
            Height = BadgeSize,
            HeightUnits = DimensionUnitType.Absolute,
            BindingContext = bindingContext,
        };
        Shapes.InsertBehindContent(badge.Visual, Shapes.CreateCircle(Palette.PlayerOne, BadgeSize));
        badge.AddChild(count);
        badge.Anchor(Gum.Wireframe.Anchor.TopRight);
        badge.X = -pillRightOffset + BadgeOverlap;
        badge.Y = PillRowY - BadgeOverlap;
        badge.SetBinding(nameof(IsVisible), binding.VisibilityProperty);
        return badge;
    }

    private Panel CreateMenuCard()
    {
        var content = new StackPanel
        {
            X = MenuCardPaddingX,
            Y = MenuCardPaddingTop,
            Width = MenuCardContentWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Spacing = MenuCardSpacing,
        };
        content.AddChild(CreateBrand());
        content.AddChild(CreateFindMatchButton());
        content.AddChild(CreateMatchSearchStatus());
        content.AddChild(CreateCenteredRow(ViewFactory.CreateLabel(
            nameof(Strings.MainMenuPrivateMatch),
            TextStyles.SectionLabel)));
        content.AddChild(CreatePrivateMatchButtons());

        var card = new Panel
        {
            Width = MenuCardWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = MenuCardPaddingBottom,
            HeightUnits = DimensionUnitType.RelativeToChildren,
        };
        card.Anchor(Gum.Wireframe.Anchor.Top);
        card.Y = MenuCardY;
        card.AddChild(content);
        Skins.ApplyCard(card.Visual);
        return card;
    }

    private static Panel CreateBrand()
    {
        var brand = new StackPanel
        {
            Spacing = BrandSpacing,
        };
        brand.AddChild(ViewFactory.CreateLabel(nameof(Strings.AppName), TextStyles.MenuTitle));
        brand.AddChild(ViewFactory.CreateLabel(nameof(Strings.LoginTagline), TextStyles.MenuTagline));
        return CreateCenteredRow(brand);
    }

    private static Panel CreateCenteredRow(FrameworkElement element)
    {
        ViewFactory.CenterHorizontally(element);

        var row = new Panel
        {
            Width = MenuCardContentWidth,
            WidthUnits = DimensionUnitType.Absolute,
            HeightUnits = DimensionUnitType.RelativeToChildren,
        };
        row.AddChild(element);
        return row;
    }

    private Button CreateFindMatchButton()
    {
        Button button = ViewFactory.CreateButton(string.Empty, MenuCardContentWidth, HeroHeight);
        ButtonSkin.CreateAndAttach(button, ButtonLooks.Hero);
        button.BindingContext = _matchSearchViewModel;
        button.SetBinding(nameof(IsEnabled), nameof(MatchSearchViewModel.CanStartSearch));
        button.Click += HandleFindMatchClick;

        button.Visual.ExposeChildrenEvents = false;
        RectangleRuntime icon = Shapes.CreateRounded(Palette.HeroIconFill, HeroIconRadius);
        icon.Anchor(Gum.Wireframe.Anchor.Left);
        icon.X = HeroPaddingX;
        icon.Width = HeroIconSize;
        icon.WidthUnits = DimensionUnitType.Absolute;
        icon.Height = HeroIconSize;
        icon.HeightUnits = DimensionUnitType.Absolute;
        button.AddChild(icon);

        var text = new StackPanel
        {
            X = HeroTextX,
            Spacing = HeroTextSpacing,
        };
        text.AddChild(ViewFactory.CreateLabel(nameof(Strings.MainMenuFindMatch), TextStyles.HeroTitle));
        text.AddChild(ViewFactory.CreateLabel(nameof(Strings.MainMenuFindMatchDetail), TextStyles.HeroDetail));
        ViewFactory.CenterVertically(text);
        button.AddChild(text);
        return button;
    }

    private Label CreateMatchSearchStatus()
    {
        Label status = ViewFactory.CreateBoundLabel(
            nameof(MatchSearchViewModel.StatusKey),
            TextStyles.FieldError,
            MenuCardContentWidth);
        status.BindingContext = _matchSearchViewModel;
        status.SetBinding(nameof(IsVisible), nameof(MatchSearchViewModel.HasStatus));
        return status;
    }

    private static StackPanel CreatePrivateMatchButtons()
    {
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = PrivateButtonSpacing,
        };
        row.AddChild(CreatePrivateMatchButton(nameof(Strings.MainMenuCreateRoom)));
        row.AddChild(CreatePrivateMatchButton(nameof(Strings.MainMenuJoinWithCode)));
        return row;
    }

    private static Button CreatePrivateMatchButton(string textKey)
    {
        Button button = ViewFactory.CreateButton(textKey, PrivateButtonWidth, PrivateButtonHeight);
        ButtonSkin.CreateAndAttach(button, ButtonLooks.Secondary);
        return button;
    }

    private MatchSearchPanel CreateMatchSearchPanel()
    {
        var panel = new MatchSearchPanel(_matchSearchViewModel);
        panel.Anchor(Gum.Wireframe.Anchor.Bottom);
        panel.Y = -SearchPanelMarginY;
        return panel;
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
        var panel = new FriendsPanel(friendsPanelViewModel);
        panel.Anchor(Gum.Wireframe.Anchor.TopRight);
        panel.X = -ScreenMetrics.MarginX;
        panel.Y = PillRowY + ScreenMetrics.MenuPillHeight + FriendsPanelGapY;
        return panel;
    }

    private static FriendRequestToast CreateRequestToast(FriendRequestsViewModel requests)
    {
        var toast = new FriendRequestToast(requests);
        toast.Anchor(Gum.Wireframe.Anchor.Top);
        toast.Y = ScreenMetrics.MarginY;
        return toast;
    }

    private void HandleFriendsClick(object? sender, EventArgs e)
    {
        _friendsPanelViewModel.Open(FriendsTab.Friends);
    }

    private void HandleChatClick(object? sender, EventArgs e)
    {
        _friendsPanelViewModel.Open(FriendsTab.Chat);
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
}
