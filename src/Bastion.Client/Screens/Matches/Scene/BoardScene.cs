using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Client.Theme;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Bastion.Client.Screens.Matches.Scene;

public sealed class BoardScene
{
    private const string ClassicBoardAssetName = "Models/tablero_clasico_9x9";
    private const string QuickBoardAssetName = "Models/tablero_clasico_7x7";
    private const string PawnAssetName = "Models/peon_clasico";
    private const byte ClassicBoardSize = 9;
    private const byte QuickBoardSize = 7;
    private const float FieldOfViewDegrees = 40f;
    private const float NearPlaneDistance = 1f;
    private const float FarPlaneDistance = 60f;

    private readonly GraphicsDevice _graphicsDevice;
    private readonly SceneModel _classicBoardModel;
    private readonly SceneModel _quickBoardModel;
    private readonly SceneModel _pawnModel;
    private readonly Dictionary<string, Color> _goalColorsByMeshName = new Dictionary<string, Color>();
    private MatchViewModel? _preparedMatch;
    private int _preparedPawnCount;

    public BoardScene(GraphicsDevice graphicsDevice, ContentManager content)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(content);

        _graphicsDevice = graphicsDevice;
        _classicBoardModel = LoadSceneModel(content, ClassicBoardAssetName);
        _quickBoardModel = LoadSceneModel(content, QuickBoardAssetName);
        _pawnModel = LoadSceneModel(content, PawnAssetName);
    }

    public void Draw(MatchViewModel match)
    {
        ArgumentNullException.ThrowIfNull(match);

        PrepareGoalColors(match);
        SceneCamera camera = CreateCamera(match.OwnTurnOrder);
        ApplyDeviceStates();
        DrawBoard(GetBoardModel(match.BoardSize), camera);
        foreach (BoardPawn pawn in match.Pawns)
        {
            DrawPawn(match.BoardSize, pawn, camera);
        }
    }

    private static SceneModel LoadSceneModel(ContentManager content, string assetName)
    {
        Model model = content.Load<Model>(assetName);
        return new SceneModel(model);
    }

    private void PrepareGoalColors(MatchViewModel match)
    {
        if (AreGoalColorsCurrent(match))
        {
            return;
        }

        _goalColorsByMeshName.Clear();
        IEnumerable<MatchPlayerViewModel> remainingPlayers = match.Players.Where(candidate => !candidate.HasLeft);
        foreach (MatchPlayerViewModel player in remainingPlayers)
        {
            string goalMeshName =
                BoardSceneLayout.GetGoalMeshName(match.BoardSize, player.TurnOrder, match.Players.Count);
            _goalColorsByMeshName[goalMeshName] = Palette.GetPawnColor(player.PawnSymbol);
        }

        _preparedMatch = match;
        _preparedPawnCount = match.Pawns.Count;
    }

    private bool AreGoalColorsCurrent(MatchViewModel match)
    {
        return ReferenceEquals(_preparedMatch, match) && _preparedPawnCount == match.Pawns.Count;
    }

    private SceneCamera CreateCamera(byte ownTurnOrder)
    {
        Vector3 cameraPosition = BoardSceneLayout.GetCameraPosition(ownTurnOrder);
        var view = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);
        var projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.ToRadians(FieldOfViewDegrees),
            _graphicsDevice.Viewport.AspectRatio,
            NearPlaneDistance,
            FarPlaneDistance);
        return new SceneCamera(view, projection);
    }

    private void ApplyDeviceStates()
    {
        _graphicsDevice.BlendState = BlendState.Opaque;
        _graphicsDevice.DepthStencilState = DepthStencilState.Default;
        _graphicsDevice.RasterizerState = RasterizerState.CullNone;
    }

    private SceneModel GetBoardModel(byte boardSize)
    {
        switch (boardSize)
        {
            case ClassicBoardSize:
                return _classicBoardModel;
            case QuickBoardSize:
                return _quickBoardModel;
            default:
                throw new ArgumentOutOfRangeException(nameof(boardSize), boardSize, "Unsupported board size.");
        }
    }

    private void DrawBoard(SceneModel boardModel, SceneCamera camera)
    {
        foreach (ModelMesh mesh in boardModel.Model.Meshes)
        {
            if (!BoardSceneLayout.IsGoalMesh(mesh.Name))
            {
                boardModel.DrawMesh(mesh, Matrix.Identity, camera);
                continue;
            }

            if (_goalColorsByMeshName.TryGetValue(mesh.Name, out Color goalColor))
            {
                SceneModel.TintMesh(mesh, goalColor);
                boardModel.DrawMesh(mesh, Matrix.Identity, camera);
            }
        }
    }

    private void DrawPawn(byte boardSize, BoardPawn pawn, SceneCamera camera)
    {
        var world = Matrix.CreateTranslation(BoardSceneLayout.GetCellCenter(boardSize, pawn.Coordinates));
        Color pawnColor = Palette.GetPawnColor(pawn.Symbol);
        foreach (ModelMesh mesh in _pawnModel.Model.Meshes)
        {
            SceneModel.TintMesh(mesh, pawnColor);
            _pawnModel.DrawMesh(mesh, world, camera);
        }
    }
}
