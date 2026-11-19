using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.GueDeriving;
using log4net;
using Microsoft.Xna.Framework;

namespace Bastion.Client.Screens.Matches;

public sealed class AbandonConfirmDialogView : DialogCard
{
    private const float NoticePaddingX = 12;
    private const float NoticePaddingY = 10;
    private const float NoticeRadius = 14;
    private const int HorizontalSideCount = 2;
    private const float NoticeTextWidth = DialogMetrics.ContentWidth - (NoticePaddingX * HorizontalSideCount);

    private static readonly ILog _logger = LogManager.GetLogger(typeof(AbandonConfirmDialogView));

    private readonly MatchAbandonmentViewModel _viewModel;

    public AbandonConfirmDialogView(MatchAbandonmentViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;
        BindingContext = viewModel;
        Anchor(Gum.Wireframe.Anchor.Center);
        Label titleLabel = ViewFactory.CreateLabel(
            nameof(Strings.MatchAbandonConfirmTitle),
            TextStyleCatalog.DialogTitle);
        Label consequenceLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(MatchAbandonmentViewModel.ConsequenceText),
            TextStyleCatalog.DialogMessage,
            DialogMetrics.ContentWidth);
        Panel waitingNoticePanel = CreateNotice(
            nameof(MatchAbandonmentViewModel.IsPreviewWaiting),
            Palette.WarningSoft);
        Panel lossNoticePanel = CreateNotice(nameof(MatchAbandonmentViewModel.IsPreviewLoss), Palette.ErrorSoft);
        Panel informationNoticePanel = CreateNotice(
            nameof(MatchAbandonmentViewModel.IsPreviewInformation),
            Palette.InfoSoft);
        Button backButton = CreateWideButton(nameof(Strings.MatchAbandonBackButton), HandleBackClick);
        Button confirmButton = CreateConfirmButton();
        Content.AddChild(titleLabel);
        Content.AddChild(consequenceLabel);
        Content.AddChild(waitingNoticePanel);
        Content.AddChild(lossNoticePanel);
        Content.AddChild(informationNoticePanel);
        Content.AddChild(backButton);
        Content.AddChild(confirmButton);
    }

    private static Panel CreateNotice(string visibilityProperty, Color fillColor)
    {
        var noticePanel = new Panel
        {
            Width = DialogMetrics.ContentWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = NoticePaddingY * HorizontalSideCount,
            HeightUnits = DimensionUnitType.RelativeToChildren,
        };
        noticePanel.SetBinding(nameof(IsVisible), visibilityProperty);
        RectangleRuntime background = ShapeFactory.CreateRounded(fillColor, NoticeRadius);
        ShapeFactory.InsertBehindContent(noticePanel.Visual, background);
        Label noticeLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(MatchAbandonmentViewModel.PreviewText),
            TextStyleCatalog.NoticeMessage,
            NoticeTextWidth);
        noticeLabel.X = NoticePaddingX;
        noticeLabel.Y = NoticePaddingY;
        noticePanel.AddChild(noticeLabel);
        return noticePanel;
    }

    private Button CreateConfirmButton()
    {
        Button confirmButton = ViewFactory.CreateButton(
            nameof(Strings.MatchAbandonConfirmButton),
            DialogMetrics.ContentWidth,
            DialogMetrics.ButtonHeight);
        ButtonSkin.CreateAndAttach(confirmButton, ButtonLookCatalog.Danger);
        confirmButton.Click += HandleConfirmClick;
        return confirmButton;
    }

    private void HandleBackClick(object? sender, EventArgs e)
    {
        _viewModel.CancelConfirmation();
    }

    private async void HandleConfirmClick(object? sender, EventArgs e)
    {
        try
        {
            await _viewModel.ConfirmAbandonment();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The match could not be left because the connection is closed.", ex);
        }
    }
}
