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
    private const int HorizontalSideCount = 2;

    public MatchPlayersPanel(MatchViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        Width = PanelWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Height = PaddingBottom;
        HeightUnits = DimensionUnitType.RelativeToChildren;

        var rows = new StackPanel
        {
            X = PaddingX,
            Y = PaddingTop,
            Spacing = RowSpacing,
        };
        foreach (MatchPlayerViewModel player in viewModel.Players)
        {
            rows.AddChild(CreatePlayerRow(player));
        }

        AddChild(rows);
        CardSkin.Apply(Visual);
    }

    private static Panel CreatePlayerRow(MatchPlayerViewModel player)
    {
        var nameColumn = new StackPanel
        {
            Width = NameWidth,
            WidthUnits = DimensionUnitType.Absolute,
        };
        nameColumn.AddChild(ViewFactory.CreateUntranslatedLabel(player.Nickname, TextStyleCatalog.IdentityName));
        Label detail = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(MatchPlayerViewModel.DetailText),
            TextStyleCatalog.IdentityDetail,
            NameWidth);
        detail.BindingContext = player;
        nameColumn.AddChild(detail);

        var content = new StackPanel
        {
            X = RowContentX,
            Orientation = Orientation.Horizontal,
            Spacing = RowContentSpacing,
        };
        content.AddChild(CreatePawn(player));
        content.AddChild(nameColumn);
        content.AddChild(ViewFactory.CreateUntranslatedLabel(player.WallsText, TextStyleCatalog.IdentityDetail));
        content.AddChild(ViewFactory.CreateUntranslatedLabel(player.ClockText, TextStyleCatalog.IdentityName));
        ViewFactory.CenterVertically(content);

        var row = new Panel
        {
            Width = PanelWidth - (PaddingX * HorizontalSideCount),
            WidthUnits = DimensionUnitType.Absolute,
            Height = RowHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        row.AddChild(CreateTurnHighlight(player));
        row.AddChild(content);
        return row;
    }

    private static Panel CreatePawn(MatchPlayerViewModel player)
    {
        var pawn = new Panel
        {
            Width = PawnSize,
            WidthUnits = DimensionUnitType.Absolute,
            Height = PawnSize,
            HeightUnits = DimensionUnitType.Absolute,
        };
        RectangleRuntime circle = ShapeFactory.CreateCircle(Palette.GetPawnColor(player.PawnSymbol), PawnSize);
        ShapeFactory.InsertBehindContent(pawn.Visual, circle);
        ViewFactory.CenterVertically(pawn);
        return pawn;
    }

    private static Panel CreateTurnHighlight(MatchPlayerViewModel player)
    {
        var highlight = new Panel
        {
            BindingContext = player,
        };
        highlight.Dock(Gum.Wireframe.Dock.Fill);
        highlight.SetBinding(nameof(IsVisible), nameof(MatchPlayerViewModel.HasTurn));
        ShapeFactory.InsertBehindContent(highlight.Visual, ShapeFactory.CreateRounded(Palette.Selected, RowRadius));
        return highlight;
    }
}
