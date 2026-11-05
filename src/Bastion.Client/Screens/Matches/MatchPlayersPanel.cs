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
    private const int PawnHalves = 2;
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
        Skins.ApplyCard(Visual);
    }

    private static Panel CreatePlayerRow(MatchPlayerViewModel player)
    {
        var nameColumn = new StackPanel
        {
            Width = NameWidth,
            WidthUnits = DimensionUnitType.Absolute,
        };
        nameColumn.AddChild(ViewFactory.CreateUntranslatedLabel(player.Nickname, TextStyles.IdentityName));
        nameColumn.AddChild(ViewFactory.CreateUntranslatedLabel(player.EloText, TextStyles.IdentityDetail));

        var content = new StackPanel
        {
            X = RowContentX,
            Orientation = Orientation.Horizontal,
            Spacing = RowContentSpacing,
        };
        content.AddChild(CreatePawn(player));
        content.AddChild(nameColumn);
        content.AddChild(ViewFactory.CreateUntranslatedLabel(player.WallsText, TextStyles.IdentityDetail));
        content.AddChild(ViewFactory.CreateUntranslatedLabel(player.ClockText, TextStyles.IdentityName));
        ViewFactory.CenterVertically(content);

        var row = new Panel
        {
            Width = PanelWidth - (PaddingX * HorizontalSideCount),
            WidthUnits = DimensionUnitType.Absolute,
            Height = RowHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        if (player.HasFirstTurn())
        {
            Shapes.InsertBehindContent(row.Visual, Shapes.CreateRounded(Palette.Selected, RowRadius));
        }

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
        RectangleRuntime circle = Shapes.CreateRounded(Palette.GetPawnColor(player.PawnSymbol), PawnSize / PawnHalves);
        Shapes.InsertBehindContent(pawn.Visual, circle);
        ViewFactory.CenterVertically(pawn);
        return pawn;
    }
}
