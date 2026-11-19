using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.GueDeriving;

namespace Bastion.Client.Screens.Matches;

public sealed class MatchEndDialogView : Panel
{
    private const float CardWidth = 500;
    private const float PaddingX = 30;
    private const float PaddingTop = 28;
    private const float PaddingBottom = 28;
    private const float SectionSpacing = 14;
    private const int HorizontalSideCount = 2;
    private const float ContentWidth = CardWidth - (PaddingX * HorizontalSideCount);
    private const float RowSpacing = 8;
    private const float RowHeight = 46;
    private const float RowRadius = 14;
    private const float RowBorderWidth = 1.5f;
    private const float RowContentX = 10;
    private const float RowContentSpacing = 10;
    private const float PlaceWidth = 22;
    private const float PawnSize = 22;
    private const float ResultOffsetX = -14;
    private const float EloPanelHeight = 76;
    private const float EloPaddingX = 16;
    private const float EloPaddingTop = 12;
    private const float EloSpacing = 6;
    private const float EloRadius = 18;
    private const float EloLineSpacing = 14;
    private const float ButtonSpacing = 12;
    private const float ButtonHeight = 52;
    private const float ButtonWidth = (ContentWidth - ButtonSpacing) / HorizontalSideCount;

    private readonly MatchEndViewModel _viewModel;
    private readonly StackPanel _rowsStackPanel;

    public MatchEndDialogView(MatchEndViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;
        Width = CardWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Height = PaddingBottom;
        HeightUnits = DimensionUnitType.RelativeToChildren;
        Anchor(Gum.Wireframe.Anchor.Center);
        BindingContext = viewModel;

        var contentStackPanel = new StackPanel
        {
            X = PaddingX,
            Y = PaddingTop,
            Spacing = SectionSpacing,
        };
        _rowsStackPanel = new StackPanel
        {
            Spacing = RowSpacing,
        };
        Label kindLabel = CreateBoundText(nameof(MatchEndViewModel.KindText), TextStyleCatalog.MatchEndKind);
        Label titleLabel = CreateBoundText(nameof(MatchEndViewModel.TitleText), TextStyleCatalog.MatchEndTitle);
        Label reasonLabel = CreateBoundText(nameof(MatchEndViewModel.ReasonText), TextStyleCatalog.MatchEndReason);
        Panel eloPanel = CreateEloPanel();
        Label privateNoticeLabel = CreatePrivateNotice();
        Label summaryLabel = CreateBoundText(nameof(MatchEndViewModel.SummaryText), TextStyleCatalog.MatchEndSummary);
        StackPanel buttonsStackPanel = CreateButtons();
        contentStackPanel.AddChild(kindLabel);
        contentStackPanel.AddChild(titleLabel);
        contentStackPanel.AddChild(reasonLabel);
        contentStackPanel.AddChild(_rowsStackPanel);
        contentStackPanel.AddChild(eloPanel);
        contentStackPanel.AddChild(privateNoticeLabel);
        contentStackPanel.AddChild(summaryLabel);
        contentStackPanel.AddChild(buttonsStackPanel);
        AddChild(contentStackPanel);
        CardSkin.Apply(Visual);
        _viewModel.RowsChanged += HandleRowsChanged;
    }

    private static Label CreateBoundText(string textProperty, TextStyle style)
    {
        return ViewFactory.CreateBoundUntranslatedLabel(textProperty, style, ContentWidth);
    }

    private static Panel CreateEloPanel()
    {
        var eloPanel = new Panel
        {
            Width = ContentWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = EloPanelHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        eloPanel.SetBinding(nameof(IsVisible), nameof(MatchEndViewModel.HasElo));
        RectangleRuntime background = ShapeFactory.CreateRounded(Palette.SurfaceHover, EloRadius);
        background.StrokeColor = Palette.Border;
        background.StrokeWidth = RowBorderWidth;
        ShapeFactory.InsertBehindContent(eloPanel.Visual, background);

        var eloLineStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = EloLineSpacing,
        };
        Label transitionLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(MatchEndViewModel.EloText),
            TextStyleCatalog.MatchEndElo);
        Label gainLabel = ViewFactory.CreateBoundNotice(
            nameof(MatchEndViewModel.EloChangeText),
            nameof(MatchEndViewModel.HasEloGain),
            TextStyleCatalog.EloGain);
        Label lossLabel = ViewFactory.CreateBoundNotice(
            nameof(MatchEndViewModel.EloChangeText),
            nameof(MatchEndViewModel.HasEloLoss),
            TextStyleCatalog.EloLoss);
        ViewFactory.CenterVertically(gainLabel);
        ViewFactory.CenterVertically(lossLabel);
        eloLineStackPanel.AddChild(transitionLabel);
        eloLineStackPanel.AddChild(gainLabel);
        eloLineStackPanel.AddChild(lossLabel);

        var eloStackPanel = new StackPanel
        {
            X = EloPaddingX,
            Y = EloPaddingTop,
            Spacing = EloSpacing,
        };
        Label eloLabel = ViewFactory.CreateLabel(nameof(Strings.MatchEndEloLabel), TextStyleCatalog.FieldLabel);
        eloStackPanel.AddChild(eloLabel);
        eloStackPanel.AddChild(eloLineStackPanel);
        eloPanel.AddChild(eloStackPanel);
        return eloPanel;
    }

    private static Label CreatePrivateNotice()
    {
        Label noticeLabel = ViewFactory.CreateToggledLabel(
            nameof(Strings.MatchEndPrivateNotice),
            nameof(MatchEndViewModel.HasPrivateNotice),
            TextStyleCatalog.NoticeMessage);
        ViewFactory.SetFixedWidth(noticeLabel, ContentWidth);
        return noticeLabel;
    }

    private StackPanel CreateButtons()
    {
        var buttonsStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = ButtonSpacing,
        };
        Button backButton = ViewFactory.CreateButton(nameof(Strings.MatchBackToMenuButton), ButtonWidth, ButtonHeight);
        ButtonSkin.CreateAndAttach(backButton, ButtonLookCatalog.Secondary);
        backButton.Click += HandleBackClick;
        Button searchButton = ViewFactory.CreateButton(
            nameof(Strings.MatchEndSearchAgainButton),
            ButtonWidth,
            ButtonHeight);
        ButtonSkin.CreateAndAttach(searchButton, ButtonLookCatalog.Primary);
        searchButton.Click += HandleSearchClick;
        buttonsStackPanel.AddChild(backButton);
        buttonsStackPanel.AddChild(searchButton);
        return buttonsStackPanel;
    }

    private void HandleBackClick(object? sender, EventArgs e)
    {
        _viewModel.RequestReturnToMenu();
    }

    private void HandleSearchClick(object? sender, EventArgs e)
    {
        _viewModel.RequestSearchAgain();
    }

    private void HandleRowsChanged(object? sender, EventArgs e)
    {
        ViewFactory.ClearRows(_rowsStackPanel.Visual);
        foreach (MatchEndRow row in _viewModel.Rows)
        {
            Panel rowPanel = CreateRow(row);
            _rowsStackPanel.AddChild(rowPanel);
        }
    }

    private static Panel CreateRow(MatchEndRow row)
    {
        var rowPanel = new Panel
        {
            Width = ContentWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = RowHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        RectangleRuntime background = ShapeFactory.CreateRounded(Palette.White, RowRadius);
        background.StrokeColor = Palette.Border;
        background.StrokeWidth = RowBorderWidth;
        ShapeFactory.InsertBehindContent(rowPanel.Visual, background);

        var contentStackPanel = new StackPanel
        {
            X = RowContentX,
            Orientation = Orientation.Horizontal,
            Spacing = RowContentSpacing,
        };
        Label placeLabel = ViewFactory.CreateUntranslatedLabel(row.PlaceText, TextStyleCatalog.ChatOwnAuthor);
        ViewFactory.SetFixedWidth(placeLabel, PlaceWidth);
        ViewFactory.CenterVertically(placeLabel);
        Panel pawnPanel = CreatePawn(row);
        contentStackPanel.AddChild(placeLabel);
        contentStackPanel.AddChild(pawnPanel);
        Label nicknameLabel = ViewFactory.CreateUntranslatedLabel(row.Nickname, TextStyleCatalog.FieldText);
        ViewFactory.CenterVertically(nicknameLabel);
        contentStackPanel.AddChild(nicknameLabel);
        if (row.IsOwn)
        {
            Label ownLabel = ViewFactory.CreateLabel(nameof(Strings.MatchEndOwnTag), TextStyleCatalog.Hint);
            ViewFactory.CenterVertically(ownLabel);
            contentStackPanel.AddChild(ownLabel);
        }

        ViewFactory.CenterVertically(contentStackPanel);
        rowPanel.AddChild(contentStackPanel);
        Label resultLabel = ViewFactory.CreateUntranslatedLabel(row.ResultText, TextStyleCatalog.SmallButton);
        resultLabel.Anchor(Gum.Wireframe.Anchor.Right);
        resultLabel.X = ResultOffsetX;
        rowPanel.AddChild(resultLabel);
        return rowPanel;
    }

    private static Panel CreatePawn(MatchEndRow row)
    {
        var pawnPanel = new Panel
        {
            Width = PawnSize,
            WidthUnits = DimensionUnitType.Absolute,
            Height = PawnSize,
            HeightUnits = DimensionUnitType.Absolute,
        };
        RectangleRuntime circle = ShapeFactory.CreateCircle(Palette.GetPawnColor(row.PawnSymbol), PawnSize);
        ShapeFactory.InsertBehindContent(pawnPanel.Visual, circle);
        ViewFactory.CenterVertically(pawnPanel);
        return pawnPanel;
    }
}
