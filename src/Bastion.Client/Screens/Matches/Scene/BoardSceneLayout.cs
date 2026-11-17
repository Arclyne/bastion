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
    public const float SurfaceHeight = 0f;
    public const float BoardCenter = 0f;
    private const float CameraHeight = 16f;
    private const float CameraDistance = 12.5f;
    private const float GrooveOffset = 0.5f;
    private const float WallLength = 1.8f;
    private const float WallThickness = 0.14f;
    private const float WallHeight = 0.45f;

    private static readonly Vector3 _surfaceCenter = new Vector3(BoardCenter, SurfaceHeight, BoardCenter);

    public static Plane Surface { get; } = new Plane(_surfaceCenter, Vector3.Up);

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
        coordinates = ToBoardCoordinates(boardSize, surfacePoint, MathF.Round);
        return BoardLayout.IsInside(boardSize, coordinates);
    }

    public static bool TryGetGroove(byte boardSize, Vector3 surfacePoint, out BoardCoordinates groove)
    {
        groove = ToBoardCoordinates(boardSize, surfacePoint, MathF.Floor);
        return WallPolicy.IsGrooveOnBoard(boardSize, groove);
    }

    public static Matrix GetWallTransform(byte boardSize, Wall wall)
    {
        ArgumentNullException.ThrowIfNull(wall);

        float centerIndex = GetCenterIndex(boardSize);
        float centerX = wall.Groove.ColumnIndex + GrooveOffset - centerIndex;
        float centerY = SurfaceHeight + (WallHeight * WallMesh.HalfSide);
        float centerZ = centerIndex - wall.Groove.RowIndex - GrooveOffset;
        bool isHorizontal = wall.Orientation == WallOrientation.Horizontal;
        float scaleX = isHorizontal ? WallLength : WallThickness;
        float scaleZ = isHorizontal ? WallThickness : WallLength;
        var scale = Matrix.CreateScale(scaleX, WallHeight, scaleZ);
        var translation = Matrix.CreateTranslation(centerX, centerY, centerZ);
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

    private static BoardCoordinates ToBoardCoordinates(byte boardSize, Vector3 surfacePoint, Func<float, float> snap)
    {
        float centerIndex = GetCenterIndex(boardSize);
        float columnPosition = surfacePoint.X + centerIndex;
        float rowPosition = centerIndex - surfacePoint.Z;
        float snappedColumn = snap(columnPosition);
        float snappedRow = snap(rowPosition);
        int columnIndex = (int)snappedColumn;
        int rowIndex = (int)snappedRow;
        var coordinates = new BoardCoordinates(columnIndex, rowIndex);
        return coordinates;
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

        int lastColumnCode = FirstColumnName + lastIndex;
        char lastColumnName = (char)lastColumnCode;
        char goalColumn = startingCoordinates.ColumnIndex == FirstIndex ? lastColumnName : FirstColumnName;
        return string.Create(CultureInfo.InvariantCulture, $"{FourPlayerGoalColumnPrefix}{goalColumn}");
    }
}
