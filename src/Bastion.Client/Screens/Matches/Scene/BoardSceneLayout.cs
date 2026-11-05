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
    private const int TwoPlayers = 2;
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

    public static Vector3 GetCameraPosition(byte ownTurnOrder)
    {
        switch (ownTurnOrder)
        {
            case Match.FirstTurnOrder:
                return new Vector3(0f, CameraHeight, CameraDistance);
            case Match.SecondTurnOrder:
                return new Vector3(0f, CameraHeight, -CameraDistance);
            case Match.ThirdTurnOrder:
                return new Vector3(-CameraDistance, CameraHeight, 0f);
            case Match.FourthTurnOrder:
                return new Vector3(CameraDistance, CameraHeight, 0f);
            default:
                throw new ArgumentOutOfRangeException(nameof(ownTurnOrder), ownTurnOrder, "Unsupported turn order.");
        }
    }

    public static string GetGoalMeshName(byte boardSize, byte turnOrder, int playerCount)
    {
        BoardCoordinates startingCoordinates = BoardLayout.GetStartingCoordinates(boardSize, turnOrder);
        if (playerCount == TwoPlayers)
        {
            int goalRowNumber = boardSize - startingCoordinates.RowIndex;
            return string.Create(CultureInfo.InvariantCulture, $"{TwoPlayerGoalMeshPrefix}{goalRowNumber}");
        }

        return GetFourPlayerGoalMeshName(boardSize, startingCoordinates);
    }

    private static string GetFourPlayerGoalMeshName(byte boardSize, BoardCoordinates startingCoordinates)
    {
        int lastIndex = boardSize - FirstRowNumber;
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

    public static bool IsGoalMesh(string meshName)
    {
        ArgumentNullException.ThrowIfNull(meshName);

        return meshName.StartsWith(GoalMeshPrefix, StringComparison.Ordinal);
    }
}
