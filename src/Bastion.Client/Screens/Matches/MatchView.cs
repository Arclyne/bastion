using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using log4net;

namespace Bastion.Client.Screens.Matches;

public sealed class MatchView : ScreenView
{
    private const float TurnPillPaddingX = 24;
    private const int HorizontalSideCount = 2;
    private const float AbandonButtonWidth = 210;
    private const float AbandonStatusGap = 8;
    private const float AbandonStatusOffsetY = -(ScreenMetrics.MarginY + ScreenMetrics.MenuPillHeight
        + AbandonStatusGap);
    private const float PlayerLeftNoticeWidth = 420;
    private const float MoveNoticeWidth = 360;
    private const float MoveNoticeHeight = 28;
    private const float WallControlsOffsetY = -(ScreenMetrics.MarginY + MoveNoticeHeight);

    private static readonly ILog _logger = LogManager.GetLogger(typeof(MatchView));

    private readonly MatchAbandonmentViewModel _abandonmentViewModel;

    public MatchView(
        MatchViewModel matchViewModel,
        MatchChatViewModel chatViewModel,
        MatchAbandonmentViewModel abandonmentViewModel)
    {
        ArgumentNullException.ThrowIfNull(matchViewModel);
        ArgumentNullException.ThrowIfNull(chatViewModel);
        ArgumentNullException.ThrowIfNull(abandonmentViewModel);

        _abandonmentViewModel = abandonmentViewModel;
        MatchPlayersPanel matchPlayersPanel = CreatePlayersPanel(matchViewModel);
        Panel turnPillPanel = CreateTurnPill(matchViewModel);
        Label moveNoticeLabel = CreateMoveNotice(matchViewModel.MatchPlay);
        WallControlsPanel wallControlsPanel = CreateWallControls(matchViewModel.WallPlacement);
        MatchChatPanel matchChatPanel = CreateChatPanel(chatViewModel);
        Button abandonButton = CreateAbandonButton();
        Label abandonStatusLabel = CreateAbandonStatus();
        Label playerLeftNoticeLabel = CreatePlayerLeftNotice();
        AddChild(matchPlayersPanel);
        AddChild(turnPillPanel);
        AddChild(moveNoticeLabel);
        AddChild(wallControlsPanel);
        AddChild(matchChatPanel);
        AddChild(abandonButton);
        AddChild(abandonStatusLabel);
        AddChild(playerLeftNoticeLabel);
        var rivalAbandonedDialogView = new RivalAbandonedDialogView(abandonmentViewModel);
        AddDialog(
            rivalAbandonedDialogView,
            abandonmentViewModel,
            nameof(MatchAbandonmentViewModel.HasRivalAbandoned));
    }

    private static MatchPlayersPanel CreatePlayersPanel(MatchViewModel matchViewModel)
    {
        var matchPlayersPanel = new MatchPlayersPanel(matchViewModel);
        matchPlayersPanel.Anchor(Gum.Wireframe.Anchor.TopLeft);
        matchPlayersPanel.X = ScreenMetrics.MarginX;
        matchPlayersPanel.Y = ScreenMetrics.MarginY;
        return matchPlayersPanel;
    }

    private static Panel CreateTurnPill(MatchViewModel matchViewModel)
    {
        var pillPanel = new Panel
        {
            Width = TurnPillPaddingX * HorizontalSideCount,
            WidthUnits = DimensionUnitType.RelativeToChildren,
            Height = ScreenMetrics.MenuPillHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        pillPanel.Anchor(Gum.Wireframe.Anchor.TopRight);
        pillPanel.X = -ScreenMetrics.MarginX;
        pillPanel.Y = ScreenMetrics.MarginY;
        Label turnLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(MatchViewModel.TurnText),
            TextStyleCatalog.IdentityName);
        turnLabel.BindingContext = matchViewModel;
        turnLabel.X = TurnPillPaddingX;
        ViewFactory.CenterVertically(turnLabel);
        pillPanel.AddChild(turnLabel);
        CardSkin.Apply(pillPanel.Visual);
        return pillPanel;
    }

    private static Label CreateMoveNotice(MatchPlayViewModel playViewModel)
    {
        Label noticeLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(MatchPlayViewModel.NoticeText),
            TextStyleCatalog.FieldError,
            MoveNoticeWidth);
        noticeLabel.BindingContext = playViewModel;
        noticeLabel.SetBinding(nameof(IsVisible), nameof(MatchPlayViewModel.HasNotice));
        noticeLabel.Anchor(Gum.Wireframe.Anchor.Bottom);
        noticeLabel.Y = -ScreenMetrics.MarginY;
        return noticeLabel;
    }

    private static WallControlsPanel CreateWallControls(WallPlacementViewModel wallPlacement)
    {
        var wallControlsPanel = new WallControlsPanel(wallPlacement);
        wallControlsPanel.Anchor(Gum.Wireframe.Anchor.Bottom);
        wallControlsPanel.Y = WallControlsOffsetY;
        return wallControlsPanel;
    }

    private static MatchChatPanel CreateChatPanel(MatchChatViewModel chatViewModel)
    {
        var matchChatPanel = new MatchChatPanel(chatViewModel);
        matchChatPanel.Anchor(Gum.Wireframe.Anchor.BottomRight);
        matchChatPanel.X = -ScreenMetrics.MarginX;
        matchChatPanel.Y = -ScreenMetrics.MarginY;
        return matchChatPanel;
    }

    private Button CreateAbandonButton()
    {
        Button button = ViewFactory.CreateButton(
            nameof(Strings.MatchAbandonButton),
            AbandonButtonWidth,
            ScreenMetrics.MenuPillHeight);
        ButtonSkin.CreateAndAttach(button, ButtonLookCatalog.Pill);
        button.BindingContext = _abandonmentViewModel;
        button.SetBinding(nameof(IsEnabled), nameof(MatchAbandonmentViewModel.CanAbandon));
        button.Anchor(Gum.Wireframe.Anchor.BottomLeft);
        button.X = ScreenMetrics.MarginX;
        button.Y = -ScreenMetrics.MarginY;
        button.Click += HandleAbandonClick;
        return button;
    }

    private async void HandleAbandonClick(object? sender, EventArgs e)
    {
        try
        {
            await _abandonmentViewModel.AbandonMatch();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The match could not be left because the connection is closed.", ex);
        }
    }

    private Label CreateAbandonStatus()
    {
        Label statusLabel = ViewFactory.CreateBoundNotice(
            nameof(MatchAbandonmentViewModel.StatusText),
            nameof(MatchAbandonmentViewModel.HasStatus),
            TextStyleCatalog.FieldError);
        ViewFactory.SetFixedWidth(statusLabel, AbandonButtonWidth);
        statusLabel.BindingContext = _abandonmentViewModel;
        statusLabel.Anchor(Gum.Wireframe.Anchor.BottomLeft);
        statusLabel.X = ScreenMetrics.MarginX;
        statusLabel.Y = AbandonStatusOffsetY;
        return statusLabel;
    }

    private Label CreatePlayerLeftNotice()
    {
        Label noticeLabel = ViewFactory.CreateBoundNotice(
            nameof(MatchAbandonmentViewModel.PlayerLeftText),
            nameof(MatchAbandonmentViewModel.HasPlayerLeft),
            TextStyleCatalog.IdentityName);
        ViewFactory.SetFixedWidth(noticeLabel, PlayerLeftNoticeWidth);
        noticeLabel.BindingContext = _abandonmentViewModel;
        noticeLabel.Anchor(Gum.Wireframe.Anchor.Top);
        noticeLabel.Y = ScreenMetrics.MarginY;
        return noticeLabel;
    }
}
