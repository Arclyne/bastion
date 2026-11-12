using System;
using System.Globalization;
using Bastion.Domain.Matches;
using Microsoft.Xna.Framework;

namespace Bastion.Client.Screens.Matches.Scene;

public static class BoardSceneLayout
{
    private const string GoalMeshPrefix = "Meta";
    private const string TwoPlayerGoalMeshPrefix = "Meta_Fila";
    private const string FourPlayerGoalRowPrefix = "Meta4J_Fila";
    private const string FourPlayerGoalColumnPrefix = "Meta4J_Columna";
    private const char FirstColumnName = 'A';
    private const int FirstRowNumber = 1;
    private const int FirstIndex = 0;
    private const float BoardHalves = 2f;
    private const float SurfaceHeight = 0f;
    private const float BoardCenter = 0f;
    private const float CameraHeight = 16f;
    private const float CameraDistance = 12.5f;
    private const float GrooveOffset = 0.5f;
    private const float WallLength = 1.8f;
    private const float WallThickness = 0.14f;
    private const float WallHeight = 0.45f;

    public static Vector3 GetCellCenter(byte boardSize, BoardCoordinates coordinates)
    {
        ArgumentNullException.ThrowIfNull(coordinates);

        float centerIndex = GetCenterIndex(boardSize);
        float columnOffset = coordinates.ColumnIndex - centerIndex;
        float rowOffset = centerIndex - coordinates.RowIndex;
        return new Vector3(columnOffset, SurfaceHeight, rowOffset);
    }

    public static bool TryGetCoordinates(byte boardSize, Vector3 surfacePoint, out BoardCoordinates coordinates)
    {
        float centerIndex = GetCenterIndex(boardSize);
        float columnPosition = surfacePoint.X + centerIndex;
        float rowPosition = centerIndex - surfacePoint.Z;
        float roundedColumn = MathF.Round(columnPosition);
        float roundedRow = MathF.Round(rowPosition);
        int columnIndex = (int)roundedColumn;
        int rowIndex = (int)roundedRow;
        coordinates = new BoardCoordinates(columnIndex, rowIndex);
        bool isColumnInside = columnIndex >= FirstIndex && columnIndex < boardSize;
        bool isRowInside = rowIndex >= FirstIndex && rowIndex < boardSize;
        return isColumnInside && isRowInside;
    }

    public static bool TryGetGroove(byte boardSize, Vector3 surfacePoint, out BoardCoordinates groove)
    {
        float centerIndex = (boardSize - 1) / BoardHalves;
        float columnPosition = surfacePoint.X + centerIndex;
        float rowPosition = centerIndex - surfacePoint.Z;
        float flooredColumn = MathF.Floor(columnPosition);
        float flooredRow = MathF.Floor(rowPosition);
        int columnIndex = (int)flooredColumn;
        int rowIndex = (int)flooredRow;
        groove = new BoardCoordinates(columnIndex, rowIndex);
        int grooveCount = boardSize - FirstRowNumber;
        bool isColumnInside = columnIndex >= FirstIndex && columnIndex < grooveCount;
        bool isRowInside = rowIndex >= FirstIndex && rowIndex < grooveCount;
        return isColumnInside && isRowInside;
    }

    public static Matrix GetWallTransform(byte boardSize, Wall wall)
    {
        ArgumentNullException.ThrowIfNull(wall);

        float centerIndex = (boardSize - 1) / BoardHalves;
        float centerX = wall.Groove.ColumnIndex + GrooveOffset - centerIndex;
        float centerZ = centerIndex - wall.Groove.RowIndex - GrooveOffset;
        bool isHorizontal = wall.Orientation == WallOrientation.Horizontal;
        float scaleX = isHorizontal ? WallLength : WallThickness;
        float scaleZ = isHorizontal ? WallThickness : WallLength;
        var scale = Matrix.CreateScale(scaleX, WallHeight, scaleZ);
        var translation = Matrix.CreateTranslation(centerX, WallHeight / BoardHalves, centerZ);
        Matrix transform = scale * translation;
        return transform;
    }

    public static Vector3 GetCameraPosition(byte ownTurnOrder)
    {
        switch (ownTurnOrder)
        {
            case Match.FirstTurnOrder:
                return new Vector3(BoardCenter, CameraHeight, CameraDistance);
            case Match.SecondTurnOrder:
                return new Vector3(BoardCenter, CameraHeight, -CameraDistance);
            case Match.ThirdTurnOrder:
                return new Vector3(-CameraDistance, CameraHeight, BoardCenter);
            case Match.FourthTurnOrder:
                return new Vector3(CameraDistance, CameraHeight, BoardCenter);
            default:
                throw new ArgumentOutOfRangeException(nameof(ownTurnOrder), ownTurnOrder, "Unsupported turn order.");
        }
    }

    public static string GetGoalMeshName(byte boardSize, byte turnOrder, int playerCount)
    {
        BoardCoordinates startingCoordinates = BoardLayout.GetStartingCoordinates(boardSize, turnOrder);
        if (playerCount == Match.TwoPlayerCount)
        {
            int goalRowNumber = boardSize - startingCoordinates.RowIndex;
            return string.Create(CultureInfo.InvariantCulture, $"{TwoPlayerGoalMeshPrefix}{goalRowNumber}");
        }

        return GetFourPlayerGoalMeshName(boardSize, startingCoordinates);
    }

    public static bool IsGoalMesh(string meshName)
    {
        ArgumentNullException.ThrowIfNull(meshName);

        return meshName.StartsWith(GoalMeshPrefix, StringComparison.Ordinal);
    }

    private static float GetCenterIndex(byte boardSize)
    {
        int lastIndex = BoardLayout.GetLastIndex(boardSize);
        float centerIndex = lastIndex / BoardHalves;
        return centerIndex;
    }

    private static string GetFourPlayerGoalMeshName(byte boardSize, BoardCoordinates startingCoordinates)
    {
        int lastIndex = BoardLayout.GetLastIndex(boardSize);
        if (startingCoordinates.RowIndex == FirstIndex)
        {
            return string.Create(CultureInfo.InvariantCulture, $"{FourPlayerGoalRowPrefix}{boardSize}");
        }

        if (startingCoordinates.RowIndex == lastIndex)
        {
            return string.Create(CultureInfo.InvariantCulture, $"{FourPlayerGoalRowPrefix}{FirstRowNumber}");
        }

        char goalColumn = startingCoordinates.ColumnIndex == FirstIndex
            ? (char)(FirstColumnName + lastIndex)
            : FirstColumnName;
        return string.Create(CultureInfo.InvariantCulture, $"{FourPlayerGoalColumnPrefix}{goalColumn}");
    }
}
