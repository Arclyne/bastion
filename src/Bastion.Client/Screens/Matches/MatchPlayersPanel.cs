using System;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.GueDeriving;

namespace Bastion.Client.Screens.Matches;

public sealed class MatchPlayersPanel : Panel
{
    private const float PanelWidth = 360;
    private const float PaddingX = 16;
    private const float PaddingTop = 14;
    private const float PaddingBottom = 14;
    private const float RowSpacing = 8;
    private const float RowHeight = 52;
    private const float RowRadius = 12;
    private const float RowContentSpacing = 12;
    private const float RowContentX = 12;
    private const float PawnSize = 18;
    private const float NameWidth = 150;
    private const float ClockWidth = 58;
    private const float ClockHeight = 30;
    private const float ClockRadius = 15;
    private const int HorizontalSideCount = 2;
    private const float RowWidth = PanelWidth - (PaddingX * HorizontalSideCount);

    public MatchPlayersPanel(MatchViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        Width = PanelWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Height = PaddingBottom;
        HeightUnits = DimensionUnitType.RelativeToChildren;

        var rowsStackPanel = new StackPanel
        {
            X = PaddingX,
            Y = PaddingTop,
            Spacing = RowSpacing,
        };
        foreach (MatchPlayerViewModel player in viewModel.Players)
        {
            Panel playerRowPanel = CreatePlayerRow(player);
            rowsStackPanel.AddChild(playerRowPanel);
        }

        AddChild(rowsStackPanel);
        CardSkin.Apply(Visual);
    }

    private static Panel CreatePlayerRow(MatchPlayerViewModel player)
    {
        var nameColumnStackPanel = new StackPanel
        {
            Width = NameWidth,
            WidthUnits = DimensionUnitType.Absolute,
        };
        Label nicknameLabel = ViewFactory.CreateUntranslatedLabel(player.Nickname, TextStyleCatalog.IdentityName);
        nameColumnStackPanel.AddChild(nicknameLabel);
        Label detailLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(MatchPlayerViewModel.DetailText),
            TextStyleCatalog.IdentityDetail,
            NameWidth);
        detailLabel.BindingContext = player;
        nameColumnStackPanel.AddChild(detailLabel);

        var contentStackPanel = new StackPanel
        {
            X = RowContentX,
            Orientation = Orientation.Horizontal,
            Spacing = RowContentSpacing,
        };
        Panel pawnPanel = CreatePawn(player);
        Label wallsLabel = CreateWallsLabel(player);
        Panel clockPanel = CreateClock(player);
        contentStackPanel.AddChild(pawnPanel);
        contentStackPanel.AddChild(nameColumnStackPanel);
        contentStackPanel.AddChild(wallsLabel);
        contentStackPanel.AddChild(clockPanel);
        ViewFactory.CenterVertically(contentStackPanel);

        var rowPanel = new Panel
        {
            Width = RowWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = RowHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        Panel turnHighlightPanel = CreateTurnHighlight(player);
        rowPanel.AddChild(turnHighlightPanel);
        rowPanel.AddChild(contentStackPanel);
        return rowPanel;
    }

    private static Panel CreatePawn(MatchPlayerViewModel player)
    {
        var pawnPanel = new Panel
        {
            Width = PawnSize,
            WidthUnits = DimensionUnitType.Absolute,
            Height = PawnSize,
            HeightUnits = DimensionUnitType.Absolute,
        };
        RectangleRuntime circle = ShapeFactory.CreateCircle(Palette.GetPawnColor(player.PawnSymbol), PawnSize);
        ShapeFactory.InsertBehindContent(pawnPanel.Visual, circle);
        ViewFactory.CenterVertically(pawnPanel);
        return pawnPanel;
    }

    private static Label CreateWallsLabel(MatchPlayerViewModel player)
    {
        Label wallsLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(MatchPlayerViewModel.WallsText),
            TextStyleCatalog.IdentityDetail);
        wallsLabel.BindingContext = player;
        return wallsLabel;
    }

    private static Panel CreateClock(MatchPlayerViewModel player)
    {
        var clockPanel = new Panel
        {
            Width = ClockWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = ClockHeight,
            HeightUnits = DimensionUnitType.Absolute,
            BindingContext = player,
        };
        var lowBackgroundPanel = new Panel();
        lowBackgroundPanel.Dock(Gum.Wireframe.Dock.Fill);
        lowBackgroundPanel.SetBinding(nameof(IsVisible), nameof(MatchPlayerViewModel.IsClockLow));
        RectangleRuntime lowBackground = ShapeFactory.CreateRounded(Palette.ErrorSoft, ClockRadius);
        ShapeFactory.InsertBehindContent(lowBackgroundPanel.Visual, lowBackground);
        clockPanel.AddChild(lowBackgroundPanel);
        AddClockFigure(clockPanel, nameof(MatchPlayerViewModel.IsClockRunning), TextStyleCatalog.RunningClock);
        AddClockFigure(clockPanel, nameof(MatchPlayerViewModel.IsClockWaiting), TextStyleCatalog.WaitingClock);
        AddClockFigure(clockPanel, nameof(MatchPlayerViewModel.IsClockLow), TextStyleCatalog.LowClock);
        ViewFactory.CenterVertically(clockPanel);
        return clockPanel;
    }

    private static void AddClockFigure(Panel clockPanel, string visibilityProperty, TextStyle style)
    {
        Label figureLabel = ViewFactory.CreateBoundNotice(
            nameof(MatchPlayerViewModel.ClockText),
            visibilityProperty,
            style);
        ViewFactory.CenterHorizontally(figureLabel);
        ViewFactory.CenterVertically(figureLabel);
        clockPanel.AddChild(figureLabel);
    }

    private static Panel CreateTurnHighlight(MatchPlayerViewModel player)
    {
        var highlightPanel = new Panel
        {
            BindingContext = player,
        };
        highlightPanel.Dock(Gum.Wireframe.Dock.Fill);
        highlightPanel.SetBinding(nameof(IsVisible), nameof(MatchPlayerViewModel.HasTurn));
        RectangleRuntime background = ShapeFactory.CreateRounded(Palette.Selected, RowRadius);
        ShapeFactory.InsertBehindContent(highlightPanel.Visual, background);
        return highlightPanel;
    }
}
