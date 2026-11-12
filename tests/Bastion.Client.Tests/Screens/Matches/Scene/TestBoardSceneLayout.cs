using Bastion.Client.Screens.Matches.Scene;
using Bastion.Domain.Matches;
using Microsoft.Xna.Framework;
using Xunit;

namespace Bastion.Client.Tests.Screens.Matches.Scene;

public sealed class TestBoardSceneLayout
{
    private const byte ClassicBoardSize = 9;
    private const byte QuickBoardSize = 7;
    private const int FirstIndex = 0;
    private const int ClassicLastIndex = 8;
    private const int QuickCenterIndex = 3;
    private const float ClassicHalfSpan = 4f;
    private const float BoardHeight = 0f;
    private const string ClassicFirstRowGoal = "Meta_Fila1";
    private const string ClassicLastRowGoal = "Meta_Fila9";
    private const string QuickLastRowGoal = "Meta_Fila7";
    private const string FourPlayerGoal = "Meta4J_ColumnaA";
    private const string FirstCell = "Casilla_a1";
    private const int TwoPlayers = 2;
    private const int FourPlayers = 4;

    [Fact]
    public void GetCellCenter_FirstCell_SitsAtNearLeftCorner()
    {
        var expectedCenter = new Vector3(-ClassicHalfSpan, BoardHeight, ClassicHalfSpan);
        var firstCell = new BoardCoordinates(FirstIndex, FirstIndex);

        Vector3 center = BoardSceneLayout.GetCellCenter(ClassicBoardSize, firstCell);

        Assert.Equal(expectedCenter, center);
    }

    [Fact]
    public void GetCellCenter_LastCell_SitsAtFarRightCorner()
    {
        var expectedCenter = new Vector3(ClassicHalfSpan, BoardHeight, -ClassicHalfSpan);
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
    public void GetCameraPosition_FirstTurnOrder_LooksFromFirstRowSide()
    {
        Vector3 cameraPosition = BoardSceneLayout.GetCameraPosition(Match.FirstTurnOrder);

        bool isOnFirstRowSide = cameraPosition.Z > 0f;
        Assert.True(isOnFirstRowSide);
    }

    [Fact]
    public void GetCameraPosition_SecondTurnOrder_LooksFromLastRowSide()
    {
        Vector3 cameraPosition = BoardSceneLayout.GetCameraPosition(Match.SecondTurnOrder);

        bool isOnLastRowSide = cameraPosition.Z < 0f;
        Assert.True(isOnLastRowSide);
    }

    [Fact]
    public void GetCameraPosition_ThirdTurnOrder_LooksFromFirstColumnSide()
    {
        Vector3 cameraPosition = BoardSceneLayout.GetCameraPosition(Match.ThirdTurnOrder);

        bool isOnFirstColumnSide = cameraPosition.X < 0f;
        Assert.True(isOnFirstColumnSide);
    }

    [Fact]
    public void GetCameraPosition_FourthTurnOrder_LooksFromLastColumnSide()
    {
        Vector3 cameraPosition = BoardSceneLayout.GetCameraPosition(Match.FourthTurnOrder);

        bool isOnLastColumnSide = cameraPosition.X > 0f;
        Assert.True(isOnLastColumnSide);
    }

    [Fact]
    public void GetGoalMeshName_FirstTurnOrder_ReturnsLastRowGoal()
    {
        string goalMeshName = BoardSceneLayout.GetGoalMeshName(ClassicBoardSize, Match.FirstTurnOrder, TwoPlayers);

        Assert.Equal(ClassicLastRowGoal, goalMeshName);
    }

    [Fact]
    public void GetGoalMeshName_SecondTurnOrder_ReturnsFirstRowGoal()
    {
        string goalMeshName = BoardSceneLayout.GetGoalMeshName(ClassicBoardSize, Match.SecondTurnOrder, TwoPlayers);

        Assert.Equal(ClassicFirstRowGoal, goalMeshName);
    }

    [Fact]
    public void GetGoalMeshName_QuickBoardFirstTurnOrder_ReturnsLastRowGoal()
    {
        string goalMeshName = BoardSceneLayout.GetGoalMeshName(QuickBoardSize, Match.FirstTurnOrder, TwoPlayers);

        Assert.Equal(QuickLastRowGoal, goalMeshName);
    }

    [Theory]
    [InlineData(Match.FirstTurnOrder, "Meta4J_Fila9")]
    [InlineData(Match.SecondTurnOrder, "Meta4J_Fila1")]
    [InlineData(Match.ThirdTurnOrder, "Meta4J_ColumnaI")]
    [InlineData(Match.FourthTurnOrder, "Meta4J_ColumnaA")]
    public void GetGoalMeshName_FourPlayers_ReturnsOppositeSideGoal(byte turnOrder, string expectedGoal)
    {
        string goalMeshName = BoardSceneLayout.GetGoalMeshName(ClassicBoardSize, turnOrder, FourPlayers);

        Assert.Equal(expectedGoal, goalMeshName);
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
