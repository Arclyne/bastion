using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using log4net;

namespace Bastion.Client.Screens.Matches;

public sealed class DrawOfferCard : Panel
{
    private const float CardWidth = 440;
    private const float PaddingX = 18;
    private const float PaddingTop = 16;
    private const float PaddingBottom = 16;
    private const float Spacing = 12;
    private const int HorizontalSideCount = 2;
    private const float ContentWidth = CardWidth - (PaddingX * HorizontalSideCount);
    private const float ButtonSpacing = 10;
    private const float ButtonWidth = (ContentWidth - ButtonSpacing) / HorizontalSideCount;
    private const float ButtonHeight = 44;
    private const float TextSpacing = 2;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(DrawOfferCard));

    private readonly MatchDrawViewModel _viewModel;

    public DrawOfferCard(MatchDrawViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;
        BindingContext = viewModel;
        Width = CardWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Height = PaddingBottom;
        HeightUnits = DimensionUnitType.RelativeToChildren;
        SetBinding(nameof(IsVisible), nameof(MatchDrawViewModel.HasRivalOffer));
        var contentStackPanel = new StackPanel
        {
            X = PaddingX,
            Y = PaddingTop,
            Spacing = Spacing,
        };
        var textStackPanel = new StackPanel
        {
            Spacing = TextSpacing,
        };
        Label titleLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(MatchDrawViewModel.RivalOfferTitleText),
            TextStyleCatalog.RivalAwayTitle,
            ContentWidth);
        Label detailLabel = ViewFactory.CreateLabel(nameof(Strings.DrawOfferDetail), TextStyleCatalog.RivalAwayDetail);
        ViewFactory.SetFixedWidth(detailLabel, ContentWidth);
        textStackPanel.AddChild(titleLabel);
        textStackPanel.AddChild(detailLabel);
        StackPanel buttonsStackPanel = CreateButtons();
        contentStackPanel.AddChild(textStackPanel);
        contentStackPanel.AddChild(buttonsStackPanel);
        AddChild(contentStackPanel);
        CardSkin.Apply(Visual);
    }

    private StackPanel CreateButtons()
    {
        var buttonsStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = ButtonSpacing,
        };
        Button acceptButton = ViewFactory.CreateButton(nameof(Strings.DrawAcceptButton), ButtonWidth, ButtonHeight);
        ButtonSkin.CreateAndAttach(acceptButton, ButtonLookCatalog.Primary);
        acceptButton.SetBinding(nameof(IsEnabled), nameof(MatchDrawViewModel.CanAnswer));
        acceptButton.Click += HandleAcceptClick;
        Button rejectButton = ViewFactory.CreateButton(nameof(Strings.DrawRejectButton), ButtonWidth, ButtonHeight);
        ButtonSkin.CreateAndAttach(rejectButton, ButtonLookCatalog.Secondary);
        rejectButton.SetBinding(nameof(IsEnabled), nameof(MatchDrawViewModel.CanAnswer));
        rejectButton.Click += HandleRejectClick;
        buttonsStackPanel.AddChild(acceptButton);
        buttonsStackPanel.AddChild(rejectButton);
        return buttonsStackPanel;
    }

    private async void HandleAcceptClick(object? sender, EventArgs e)
    {
        try
        {
            await _viewModel.AcceptDraw();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The draw could not be accepted because the connection is closed.", ex);
        }
    }

    private async void HandleRejectClick(object? sender, EventArgs e)
    {
        try
        {
            await _viewModel.RejectDraw();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The draw could not be declined because the connection is closed.", ex);
        }
    }
}
