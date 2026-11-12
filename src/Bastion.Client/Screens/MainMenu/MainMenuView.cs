using System;
using Bastion.Client.Localization;
using Bastion.Client.Screens.MainMenu.Chat;
using Bastion.Client.Screens.MainMenu.Friends;
using Bastion.Client.Theme;
using Gum.Converters;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
using Gum.GueDeriving;
using Gum.Wireframe;
using log4net;
using RenderingLibrary.Graphics;

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
    private const float TabletWidth = 460;
    private const float TabletY = 164;
    private const float TabletPaddingX = 30;
    private const float TabletPaddingTop = 26;
    private const float TabletPaddingBottom = 30;
    private const float TabletContentWidth = 400;
    private const float TabletSpacing = 20;
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
    private const float FriendsPanelGapY = 12;
    private const float BadgeSize = 22;
    private const float BadgeOverlap = 8;
    private const float SearchPanelMarginY = 88;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(MainMenuView));

    private readonly FriendsPanelViewModel _friendsPanel;
    private readonly MatchSearchViewModel _matchSearchViewModel;

    public MainMenuView(
        MainMenuViewModel viewModel,
        FriendsPanelViewModel friendsPanel,
        MatchSearchViewModel matchSearchViewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(friendsPanel);
        ArgumentNullException.ThrowIfNull(matchSearchViewModel);

        _friendsPanel = friendsPanel;
        _matchSearchViewModel = matchSearchViewModel;
        BindingContext = viewModel;
        AddChild(CreateIdentityChip());
        AddChild(CreateSettingsButton());
        AddChild(CreateFriendsButton());
        AddChild(CreateChatButton());
        AddChild(CreateCountBadge(
            friendsPanel.Requests,
            new BadgeBinding(
                nameof(FriendRequestsViewModel.PendingCountText),
                nameof(FriendRequestsViewModel.HasPending)),
            ScreenMetrics.MarginX + SettingsWidth + PillSpacing));
        AddChild(CreateCountBadge(
            friendsPanel.Chat,
            new BadgeBinding(nameof(ChatTabViewModel.UnreadText), nameof(ChatTabViewModel.HasUnread)),
            ScreenMetrics.MarginX + SettingsWidth + PillSpacing + FriendsPillWidth + PillSpacing));
        AddChild(CreateTablet());
        AddChild(CreateMatchSearchPanel());
        AddChild(CreateHowToPlayButton());
        AddChild(ViewFactory.CreateVersionLabel());
        AddChild(CreateFriendsPanel(friendsPanel));
        AddChild(CreateRequestToast(friendsPanel.Requests));
        AddDialog(
            new RemoveFriendDialogView(friendsPanel.Actions),
            friendsPanel.Actions,
            nameof(FriendActionsViewModel.IsDialogOpen));
        AddDialog(
            new PresenceDialogView(matchSearchViewModel),
            matchSearchViewModel,
            nameof(MatchSearchViewModel.IsPresenceDialogOpen));
    }

    public void ReceiveEscapeKey()
    {
        _friendsPanel.Close();
    }

    private static Panel CreateIdentityChip()
    {
        var content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = ChipSpacing };
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
        RectangleRuntime circle = Shapes.CreateRounded(Palette.AvatarFill, AvatarSize / 2);
        circle.StrokeColor = Palette.PlayerOne;
        circle.StrokeWidth = AvatarRingWidth;
        Shapes.InsertBehindContent(avatar.Visual, circle);
        return avatar;
    }

    private static StackPanel CreateIdentityText()
    {
        Label nickname = ViewFactory.CreateLabel(string.Empty, TextStyles.IdentityName);
        ((LabelVisual)nickname.Visual).LocalizeText = false;
        nickname.SetBinding(nameof(Label.Text), nameof(MainMenuViewModel.Nickname));

        var text = new StackPanel { Spacing = IdentitySpacing };
        text.AddChild(nickname);
        text.AddChild(ViewFactory.CreateLabel(nameof(Strings.MainMenuClassicMode), TextStyles.IdentityDetail));
        ViewFactory.CenterVertically(text);
        return text;
    }

    private static Button CreateSettingsButton()
    {
        Button button = CreatePillButton(nameof(Strings.MainMenuSettings), SettingsWidth);
        button.Anchor(Gum.Wireframe.Anchor.TopRight);
        button.X = -ScreenMetrics.MarginX;
        button.Y = ScreenMetrics.MarginY + SettingsOffsetY;
        return button;
    }

    private static FriendsPanel CreateFriendsPanel(FriendsPanelViewModel friendsPanel)
    {
        var panel = new FriendsPanel(friendsPanel);
        panel.Anchor(Gum.Wireframe.Anchor.TopRight);
        panel.X = -ScreenMetrics.MarginX;
        panel.Y = ScreenMetrics.MarginY + SettingsOffsetY + ScreenMetrics.MenuPillHeight + FriendsPanelGapY;
        return panel;
    }

    private static Panel CreateCountBadge(object context, BadgeBinding binding, float pillRightOffset)
    {
        Label count = ViewFactory.CreateLabel(string.Empty, TextStyles.PrimaryButton);
        ((LabelVisual)count.Visual).LocalizeText = false;
        count.SetBinding(nameof(Label.Text), binding.TextProperty);
        count.XUnits = GeneralUnitType.PixelsFromMiddle;
        count.XOrigin = HorizontalAlignment.Center;
        ViewFactory.CenterVertically(count);

        var badge = new Panel
        {
            Width = BadgeSize,
            WidthUnits = DimensionUnitType.Absolute,
            Height = BadgeSize,
            HeightUnits = DimensionUnitType.Absolute,
            BindingContext = context,
        };
        Shapes.InsertBehindContent(badge.Visual, Shapes.CreateRounded(Palette.PlayerOne, BadgeSize / 2));
        badge.AddChild(count);
        badge.Anchor(Gum.Wireframe.Anchor.TopRight);
        badge.X = -pillRightOffset + BadgeOverlap;
        badge.Y = ScreenMetrics.MarginY + SettingsOffsetY - BadgeOverlap;
        badge.SetBinding(nameof(IsVisible), binding.VisibilityProperty);
        return badge;
    }

    private static FriendRequestToast CreateRequestToast(FriendRequestsViewModel requests)
    {
        var toast = new FriendRequestToast(requests);
        toast.Anchor(Gum.Wireframe.Anchor.Top);
        toast.Y = ScreenMetrics.MarginY;
        return toast;
    }

    private Button CreateFriendsButton()
    {
        Button button = CreatePillButton(nameof(Strings.MainMenuFriends), FriendsPillWidth);
        button.Anchor(Gum.Wireframe.Anchor.TopRight);
        button.X = -(ScreenMetrics.MarginX + SettingsWidth + PillSpacing);
        button.Y = ScreenMetrics.MarginY + SettingsOffsetY;
        button.Click += HandleFriendsClick;
        return button;
    }

    private Button CreateChatButton()
    {
        Button button = CreatePillButton(nameof(Strings.MainMenuChat), ChatPillWidth);
        button.Anchor(Gum.Wireframe.Anchor.TopRight);
        button.X = -(ScreenMetrics.MarginX + SettingsWidth + PillSpacing + FriendsPillWidth + PillSpacing);
        button.Y = ScreenMetrics.MarginY + SettingsOffsetY;
        button.Click += HandleChatClick;
        return button;
    }

    private void HandleFriendsClick(object? sender, EventArgs e)
    {
        _friendsPanel.Open(FriendsTab.Friends);
    }

    private void HandleChatClick(object? sender, EventArgs e)
    {
        _friendsPanel.Open(FriendsTab.Chat);
    }

    private static Button CreateHowToPlayButton()
    {
        Button button = CreatePillButton(nameof(Strings.MainMenuHowToPlay), HowToPlayWidth);
        button.Anchor(Gum.Wireframe.Anchor.BottomLeft);
        button.X = ScreenMetrics.MarginX;
        button.Y = -ScreenMetrics.MarginY;
        return button;
    }

    private static Button CreatePillButton(string textKey, float width)
    {
        Button button = ViewFactory.CreateButton(textKey, width, ScreenMetrics.MenuPillHeight);
        ButtonSkin.CreateAndAttach(button, ButtonLooks.Pill);
        return button;
    }

    private Panel CreateTablet()
    {
        var content = new StackPanel
        {
            X = TabletPaddingX,
            Y = TabletPaddingTop,
            Width = TabletContentWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Spacing = TabletSpacing,
        };
        content.AddChild(CreateBrand());
        content.AddChild(CreateFindMatchButton());
        content.AddChild(CreateMatchSearchStatus());
        content.AddChild(CreateCentered(ViewFactory.CreateLabel(
            nameof(Strings.MainMenuPrivateMatch),
            TextStyles.SectionLabel)));
        content.AddChild(CreatePrivateMatchButtons());

        var tablet = new Panel
        {
            Width = TabletWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = TabletPaddingBottom,
            HeightUnits = DimensionUnitType.RelativeToChildren,
        };
        tablet.Anchor(Gum.Wireframe.Anchor.Top);
        tablet.Y = TabletY;
        tablet.AddChild(content);
        Skins.ApplyCard(tablet.Visual);
        return tablet;
    }

    private static Panel CreateBrand()
    {
        var brand = new StackPanel { Spacing = BrandSpacing };
        brand.AddChild(ViewFactory.CreateLabel(nameof(Strings.AppName), TextStyles.MenuTitle));
        brand.AddChild(ViewFactory.CreateLabel(nameof(Strings.LoginTagline), TextStyles.MenuTagline));
        return CreateCentered(brand);
    }

    private Button CreateFindMatchButton()
    {
        Button button = ViewFactory.CreateButton(string.Empty, TabletContentWidth, HeroHeight);
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

        var text = new StackPanel { X = HeroTextX, Spacing = HeroTextSpacing };
        text.AddChild(ViewFactory.CreateLabel(nameof(Strings.MainMenuFindMatch), TextStyles.HeroTitle));
        text.AddChild(ViewFactory.CreateLabel(nameof(Strings.MainMenuFindMatchDetail), TextStyles.HeroDetail));
        ViewFactory.CenterVertically(text);
        button.AddChild(text);
        return button;
    }

    private static StackPanel CreatePrivateMatchButtons()
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = PrivateButtonSpacing };
        row.AddChild(CreateSecondaryButton(nameof(Strings.MainMenuCreateRoom)));
        row.AddChild(CreateSecondaryButton(nameof(Strings.MainMenuJoinWithCode)));
        return row;
    }

    private static Button CreateSecondaryButton(string textKey)
    {
        Button button = ViewFactory.CreateButton(textKey, PrivateButtonWidth, PrivateButtonHeight);
        ButtonSkin.CreateAndAttach(button, ButtonLooks.Secondary);
        return button;
    }

    private static Panel CreateCentered(FrameworkElement element)
    {
        element.XUnits = GeneralUnitType.PixelsFromMiddle;
        element.XOrigin = HorizontalAlignment.Center;
        element.X = 0;

        var row = new Panel
        {
            Width = TabletContentWidth,
            WidthUnits = DimensionUnitType.Absolute,
            HeightUnits = DimensionUnitType.RelativeToChildren,
        };
        row.AddChild(element);
        return row;
    }

    private Label CreateMatchSearchStatus()
    {
        Label status = ViewFactory.CreateBoundLabel(
            nameof(MatchSearchViewModel.StatusKey),
            TextStyles.FieldError,
            TabletContentWidth);
        status.BindingContext = _matchSearchViewModel;
        status.SetBinding(nameof(IsVisible), nameof(MatchSearchViewModel.HasStatus));
        return status;
    }

    private MatchSearchPanel CreateMatchSearchPanel()
    {
        var panel = new MatchSearchPanel(_matchSearchViewModel);
        panel.Anchor(Gum.Wireframe.Anchor.Bottom);
        panel.Y = -SearchPanelMarginY;
        return panel;
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
