using System;
using Bastion.Client.Theme;
using Bastion.Domain.Matches;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.GueDeriving;
using Microsoft.Xna.Framework;

namespace Bastion.Client.Screens.Matches;

public sealed class BoardView : Panel
{
    private const float CellSize = 44;
    private const float CellGap = 6;
    private const float CellRadius = 8;
    private const float CellBorderWidth = 1.5f;
    private const float PawnSize = 32;
    private const float BoardPadding = 18;
    private const int PawnHalves = 2;
    private const int BoardSideCount = 2;

    public BoardView(MatchViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        float boardLength = MeasureBoardLength(viewModel.BoardSize);
        Width = boardLength + (BoardPadding * BoardSideCount);
        WidthUnits = DimensionUnitType.Absolute;
        Height = Width;
        HeightUnits = DimensionUnitType.Absolute;
        Skins.ApplyCard(Visual);

        for (int rowIndex = 0; rowIndex < viewModel.BoardSize; rowIndex++)
        {
            for (int columnIndex = 0; columnIndex < viewModel.BoardSize; columnIndex++)
            {
                var coordinates = new BoardCoordinates(columnIndex, rowIndex);
                AddChild(CreateCell(ToDisplayCoordinates(viewModel, coordinates)));
            }
        }

        foreach (BoardPawn pawn in viewModel.Pawns)
        {
            AddChild(CreatePawn(pawn.Symbol, ToDisplayCoordinates(viewModel, pawn.Coordinates)));
        }
    }

    private static float MeasureBoardLength(byte boardSize)
    {
        return (boardSize * CellSize) + ((boardSize - 1) * CellGap);
    }

    private static BoardCoordinates ToDisplayCoordinates(MatchViewModel viewModel, BoardCoordinates coordinates)
    {
        int lastIndex = viewModel.BoardSize - 1;

        // The own starting row is drawn at the bottom: the second player sees the board turned half a circle.
        if (viewModel.IsBoardRotated)
        {
            return new BoardCoordinates(lastIndex - coordinates.ColumnIndex, coordinates.RowIndex);
        }

        return new BoardCoordinates(coordinates.ColumnIndex, lastIndex - coordinates.RowIndex);
    }

    private static float ToCellOffset(int cellIndex)
    {
        return BoardPadding + (cellIndex * (CellSize + CellGap));
    }

    private static RectangleRuntime CreateCell(BoardCoordinates displayCoordinates)
    {
        RectangleRuntime cell = CreateFixedSizeShape(Palette.Background, CellRadius, CellSize);
        cell.StrokeColor = Palette.Border;
        cell.StrokeWidth = CellBorderWidth;
        cell.X = ToCellOffset(displayCoordinates.ColumnIndex);
        cell.Y = ToCellOffset(displayCoordinates.RowIndex);
        return cell;
    }

    private static RectangleRuntime CreatePawn(PawnSymbol symbol, BoardCoordinates displayCoordinates)
    {
        float inset = (CellSize - PawnSize) / PawnHalves;
        RectangleRuntime piece = CreateFixedSizeShape(Palette.GetPawnColor(symbol), PawnSize / PawnHalves, PawnSize);
        piece.X = ToCellOffset(displayCoordinates.ColumnIndex) + inset;
        piece.Y = ToCellOffset(displayCoordinates.RowIndex) + inset;
        return piece;
    }

    private static RectangleRuntime CreateFixedSizeShape(Color fill, float cornerRadius, float size)
    {
        RectangleRuntime shape = Shapes.CreateRounded(fill, cornerRadius);

        // CreateRounded docks the shape to fill its parent; board pieces are placed from the top-left corner.
        shape.Anchor(Gum.Wireframe.Anchor.TopLeft);
        shape.Width = size;
        shape.WidthUnits = DimensionUnitType.Absolute;
        shape.Height = size;
        shape.HeightUnits = DimensionUnitType.Absolute;
        return shape;
    }
}
