using Bastion.Client.Screens.Matches.Scene;
using Bastion.Domain.Matches;
using Microsoft.Xna.Framework;
using Xunit;

namespace Bastion.Client.Tests.Screens.Matches.Scene;

public class TestBoardSceneLayout
{
    private const byte ClassicBoardSize = 9;
    private const byte QuickBoardSize = 7;
    private const int ClassicLastIndex = 8;
    private const int QuickCenterIndex = 3;
    private const float ClassicHalfSpan = 4f;
    private const string ClassicFirstRowGoal = "Meta_Fila1";
    private const string ClassicLastRowGoal = "Meta_Fila9";
    private const string QuickLastRowGoal = "Meta_Fila7";
    private const string FourPlayerGoal = "Meta4J_ColumnaA";
    private const string FirstCell = "Casilla_a1";

    [Fact]
    public void GetCellCenter_FirstCell_SitsAtNearLeftCorner()
    {
        var expectedCenter = new Vector3(-ClassicHalfSpan, 0f, ClassicHalfSpan);

        Vector3 center = BoardSceneLayout.GetCellCenter(ClassicBoardSize, new BoardCoordinates(0, 0));

        Assert.Equal(expectedCenter, center);
    }

    [Fact]
    public void GetCellCenter_LastCell_SitsAtFarRightCorner()
    {
        var expectedCenter = new Vector3(ClassicHalfSpan, 0f, -ClassicHalfSpan);
        var lastCell = new BoardCoordinates(ClassicLastIndex, ClassicLastIndex);

        Vector3 center = BoardSceneLayout.GetCellCenter(ClassicBoardSize, lastCell);

        Assert.Equal(expectedCenter, center);
    }

    [Fact]
    public void GetCellCenter_QuickBoardCenterCell_SitsAtOrigin()
    {
        var centerCell = new BoardCoordinates(QuickCenterIndex, QuickCenterIndex);

        Vector3 center = BoardSceneLayout.GetCellCenter(QuickBoardSize, centerCell);

        Assert.Equal(Vector3.Zero, center);
    }

    [Fact]
    public void GetCameraPosition_UnrotatedBoard_LooksFromFirstRowSide()
    {
        Vector3 cameraPosition = BoardSceneLayout.GetCameraPosition(false);

        Assert.True(cameraPosition.Z > 0f);
    }

    [Fact]
    public void GetCameraPosition_RotatedBoard_LooksFromLastRowSide()
    {
        Vector3 cameraPosition = BoardSceneLayout.GetCameraPosition(true);

        Assert.True(cameraPosition.Z < 0f);
    }

    [Fact]
    public void GetGoalMeshName_FirstTurnOrder_ReturnsLastRowGoal()
    {
        string goalMeshName = BoardSceneLayout.GetGoalMeshName(ClassicBoardSize, Match.FirstTurnOrder);

        Assert.Equal(ClassicLastRowGoal, goalMeshName);
    }

    [Fact]
    public void GetGoalMeshName_SecondTurnOrder_ReturnsFirstRowGoal()
    {
        string goalMeshName = BoardSceneLayout.GetGoalMeshName(ClassicBoardSize, Match.SecondTurnOrder);

        Assert.Equal(ClassicFirstRowGoal, goalMeshName);
    }

    [Fact]
    public void GetGoalMeshName_QuickBoardFirstTurnOrder_ReturnsLastRowGoal()
    {
        string goalMeshName = BoardSceneLayout.GetGoalMeshName(QuickBoardSize, Match.FirstTurnOrder);

        Assert.Equal(QuickLastRowGoal, goalMeshName);
    }

    [Fact]
    public void IsGoalMesh_FourPlayerGoal_ReturnsTrue()
    {
        bool isGoalMesh = BoardSceneLayout.IsGoalMesh(FourPlayerGoal);

        Assert.True(isGoalMesh);
    }

    [Fact]
    public void IsGoalMesh_Cell_ReturnsFalse()
    {
        bool isGoalMesh = BoardSceneLayout.IsGoalMesh(FirstCell);

        Assert.False(isGoalMesh);
    }
}
