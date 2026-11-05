using System;
using System.Collections.Generic;
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

    public BoardScene(GraphicsDevice graphicsDevice, ContentManager content)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(content);

        _graphicsDevice = graphicsDevice;
        _classicBoardModel = new SceneModel(content.Load<Model>(ClassicBoardAssetName));
        _quickBoardModel = new SceneModel(content.Load<Model>(QuickBoardAssetName));
        _pawnModel = new SceneModel(content.Load<Model>(PawnAssetName));
    }

    public void Draw(MatchViewModel match)
    {
        ArgumentNullException.ThrowIfNull(match);

        PrepareGoalColors(match);
        SceneCamera camera = CreateCamera(match.IsBoardRotated);
        ApplyDeviceStates();
        DrawBoard(GetBoardModel(match.BoardSize), camera);
        foreach (BoardPawn pawn in match.Pawns)
        {
            DrawPawn(match.BoardSize, pawn, camera);
        }
    }

    private void PrepareGoalColors(MatchViewModel match)
    {
        if (ReferenceEquals(_preparedMatch, match))
        {
            return;
        }

        _goalColorsByMeshName.Clear();
        foreach (MatchPlayerViewModel player in match.Players)
        {
            string goalMeshName = BoardSceneLayout.GetGoalMeshName(match.BoardSize, player.TurnOrder);
            _goalColorsByMeshName[goalMeshName] = Palette.GetPawnColor(player.PawnSymbol);
        }

        _preparedMatch = match;
    }

    private SceneCamera CreateCamera(bool isBoardRotated)
    {
        Vector3 cameraPosition = BoardSceneLayout.GetCameraPosition(isBoardRotated);
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
