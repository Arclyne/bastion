using System;
using System.Globalization;
using Bastion.Domain.Matches;
using Microsoft.Xna.Framework;

namespace Bastion.Client.Screens.Matches.Scene;

public static class BoardSceneLayout
{
    private const string GoalMeshPrefix = "Meta";
    private const string TwoPlayerGoalMeshPrefix = "Meta_Fila";
    private const float BoardHalves = 2f;
    private const float SurfaceHeight = 0f;
    private const float CameraHeight = 16f;
    private const float CameraDistance = 12.5f;

    public static Vector3 GetCellCenter(byte boardSize, BoardCoordinates coordinates)
    {
        ArgumentNullException.ThrowIfNull(coordinates);

        float centerIndex = (boardSize - 1) / BoardHalves;
        return new Vector3(
            coordinates.ColumnIndex - centerIndex,
            SurfaceHeight,
            centerIndex - coordinates.RowIndex);
    }

    public static Vector3 GetCameraPosition(bool isBoardRotated)
    {
        float cameraSide = isBoardRotated ? -CameraDistance : CameraDistance;
        return new Vector3(0f, CameraHeight, cameraSide);
    }

    public static string GetGoalMeshName(byte boardSize, byte turnOrder)
    {
        BoardCoordinates startingCoordinates = BoardLayout.GetStartingCoordinates(boardSize, turnOrder);
        int goalRowNumber = boardSize - startingCoordinates.RowIndex;
        return string.Create(CultureInfo.InvariantCulture, $"{TwoPlayerGoalMeshPrefix}{goalRowNumber}");
    }

    public static bool IsGoalMesh(string meshName)
    {
        ArgumentNullException.ThrowIfNull(meshName);

        return meshName.StartsWith(GoalMeshPrefix, StringComparison.Ordinal);
    }
}
