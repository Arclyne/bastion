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
    private const float PlayerLeftNoticeWidth = 420;
    private const float MoveNoticeWidth = 360;

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
        AddChild(CreatePlayersPanel(matchViewModel));
        AddChild(CreateTurnPill(matchViewModel));
        AddChild(CreateMoveNotice(matchViewModel.Play));
        AddChild(CreateChatPanel(chatViewModel));
        AddChild(CreateAbandonButton());
        AddChild(CreateAbandonStatus());
        AddChild(CreatePlayerLeftNotice());
        AddDialog(
            new RivalAbandonedDialogView(abandonmentViewModel),
            abandonmentViewModel,
            nameof(MatchAbandonmentViewModel.HasRivalAbandoned));
    }

    private static MatchPlayersPanel CreatePlayersPanel(MatchViewModel matchViewModel)
    {
        var panel = new MatchPlayersPanel(matchViewModel);
        panel.Anchor(Gum.Wireframe.Anchor.TopLeft);
        panel.X = ScreenMetrics.MarginX;
        panel.Y = ScreenMetrics.MarginY;
        return panel;
    }

    private static Panel CreateTurnPill(MatchViewModel matchViewModel)
    {
        var pill = new Panel
        {
            Width = TurnPillPaddingX * HorizontalSideCount,
            WidthUnits = DimensionUnitType.RelativeToChildren,
            Height = ScreenMetrics.MenuPillHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        pill.Anchor(Gum.Wireframe.Anchor.TopRight);
        pill.X = -ScreenMetrics.MarginX;
        pill.Y = ScreenMetrics.MarginY;
        Label text = ViewFactory.CreateUntranslatedLabel(string.Empty, TextStyles.IdentityName);
        text.BindingContext = matchViewModel;
        text.SetBinding(nameof(Label.Text), nameof(MatchViewModel.TurnText));
        text.X = TurnPillPaddingX;
        ViewFactory.CenterVertically(text);
        pill.AddChild(text);
        Skins.ApplyCard(pill.Visual);
        return pill;
    }

    private static Label CreateMoveNotice(MatchPlayViewModel playViewModel)
    {
        Label notice = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(MatchPlayViewModel.NoticeText),
            TextStyles.FieldError,
            MoveNoticeWidth);
        notice.BindingContext = playViewModel;
        notice.SetBinding(nameof(IsVisible), nameof(MatchPlayViewModel.HasNotice));
        notice.Anchor(Gum.Wireframe.Anchor.Bottom);
        notice.Y = -ScreenMetrics.MarginY;
        return notice;
    }

    private static MatchChatPanel CreateChatPanel(MatchChatViewModel chatViewModel)
    {
        var panel = new MatchChatPanel(chatViewModel);
        panel.Anchor(Gum.Wireframe.Anchor.BottomRight);
        panel.X = -ScreenMetrics.MarginX;
        panel.Y = -ScreenMetrics.MarginY;
        return panel;
    }

    private Button CreateAbandonButton()
    {
        Button button = ViewFactory.CreateButton(
            nameof(Strings.MatchAbandonButton),
            AbandonButtonWidth,
            ScreenMetrics.MenuPillHeight);
        ButtonSkin.CreateAndAttach(button, ButtonLooks.Pill);
        button.BindingContext = _abandonmentViewModel;
        button.SetBinding(nameof(IsEnabled), nameof(MatchAbandonmentViewModel.CanAbandon));
        button.Anchor(Gum.Wireframe.Anchor.BottomLeft);
        button.X = ScreenMetrics.MarginX;
        button.Y = -ScreenMetrics.MarginY;
        button.Click += HandleAbandonClick;
        return button;
    }

    private Label CreateAbandonStatus()
    {
        Label status = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(MatchAbandonmentViewModel.StatusText),
            TextStyles.FieldError,
            AbandonButtonWidth);
        status.BindingContext = _abandonmentViewModel;
        status.SetBinding(nameof(IsVisible), nameof(MatchAbandonmentViewModel.HasStatus));
        status.Anchor(Gum.Wireframe.Anchor.BottomLeft);
        status.X = ScreenMetrics.MarginX;
        status.Y = -(ScreenMetrics.MarginY + ScreenMetrics.MenuPillHeight + AbandonStatusGap);
        return status;
    }

    private Label CreatePlayerLeftNotice()
    {
        Label notice = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(MatchAbandonmentViewModel.PlayerLeftText),
            TextStyles.IdentityName,
            PlayerLeftNoticeWidth);
        notice.BindingContext = _abandonmentViewModel;
        notice.SetBinding(nameof(IsVisible), nameof(MatchAbandonmentViewModel.HasPlayerLeft));
        notice.Anchor(Gum.Wireframe.Anchor.Top);
        notice.Y = ScreenMetrics.MarginY;
        return notice;
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
}
