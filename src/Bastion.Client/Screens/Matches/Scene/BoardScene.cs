using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Client.Theme;
using Bastion.Domain.Matches;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Bastion.Client.Screens.Matches.Scene;

public sealed class BoardScene : IDisposable
{
    private const string ClassicBoardAssetName = "Models/tablero_clasico_9x9";
    private const string QuickBoardAssetName = "Models/tablero_clasico_7x7";
    private const string PawnAssetName = "Models/peon_clasico";
    private const byte ClassicBoardSize = 9;
    private const byte QuickBoardSize = 7;
    private const float FieldOfViewDegrees = 40f;
    private const float NearPlaneDistance = 1f;
    private const float FarPlaneDistance = 60f;
    private const float NearScreenDepth = 0f;
    private const float FarScreenDepth = 1f;
    private const float HighlightHalfSize = 0.4f;
    private const float HighlightHeight = 0.02f;
    private const int FirstVertex = 0;
    private const int QuadTriangleCount = 2;
    private const float SelectedPawnLightening = 0.35f;
    private const float OpaqueAlpha = 1f;
    private const float GhostAlpha = 0.55f;

    private static readonly VertexPositionColor[] _highlightVertices = CreateHighlightQuad();

    private readonly GraphicsDevice _graphicsDevice;
    private readonly SceneModel _classicBoardModel;
    private readonly SceneModel _quickBoardModel;
    private readonly SceneModel _pawnModel;
    private readonly BasicEffect _highlightEffect;
    private readonly BasicEffect _wallEffect;
    private readonly Dictionary<string, Color> _goalColorsByMeshName;
    private MatchViewModel? _preparedMatch;
    private int _preparedPawnCount;

    public BoardScene(GraphicsDevice graphicsDevice, ContentManager content)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(content);

        _graphicsDevice = graphicsDevice;
        _goalColorsByMeshName = new Dictionary<string, Color>();
        _classicBoardModel = LoadSceneModel(content, ClassicBoardAssetName);
        _quickBoardModel = LoadSceneModel(content, QuickBoardAssetName);
        _pawnModel = LoadSceneModel(content, PawnAssetName);
        _highlightEffect = new BasicEffect(graphicsDevice)
        {
            VertexColorEnabled = true,
        };
        _wallEffect = new BasicEffect(graphicsDevice)
        {
            PreferPerPixelLighting = true,
        };
        _wallEffect.EnableDefaultLighting();
    }

    public void Draw(MatchViewModel match)
    {
        ArgumentNullException.ThrowIfNull(match);

        PrepareGoalColors(match);
        SceneCamera camera = CreateCamera(match.OwnTurnOrder);
        ApplyDeviceStates();
        DrawBoard(GetBoardModel(match.BoardSize), camera);
        DrawReachableSquares(match, camera);
        DrawWalls(match, camera);
        foreach (BoardPawn pawn in match.Pawns)
        {
            DrawPawn(match, pawn, camera);
        }
    }

    public BoardCoordinates FindSquareUnderCursor(MatchViewModel match, Point cursorPosition)
    {
        ArgumentNullException.ThrowIfNull(match);

        Vector3 surfacePoint = FindSurfacePoint(match, cursorPosition);
        return BoardSceneLayout.FindCoordinates(match.BoardSize, surfacePoint);
    }

    public BoardCoordinates FindGrooveUnderCursor(MatchViewModel match, Point cursorPosition)
    {
        ArgumentNullException.ThrowIfNull(match);

        Vector3 surfacePoint = FindSurfacePoint(match, cursorPosition);
        return BoardSceneLayout.FindGroove(match.BoardSize, surfacePoint);
    }

    public void Dispose()
    {
        _highlightEffect.Dispose();
        _wallEffect.Dispose();
    }

    private static VertexPositionColor[] CreateHighlightQuad()
    {
        var nearLeftCorner = new Vector3(-HighlightHalfSize, HighlightHeight, HighlightHalfSize);
        var nearRightCorner = new Vector3(HighlightHalfSize, HighlightHeight, HighlightHalfSize);
        var farLeftCorner = new Vector3(-HighlightHalfSize, HighlightHeight, -HighlightHalfSize);
        var farRightCorner = new Vector3(HighlightHalfSize, HighlightHeight, -HighlightHalfSize);
        Color color = Palette.ReachableSquare;
        var nearLeft = new VertexPositionColor(nearLeftCorner, color);
        var nearRight = new VertexPositionColor(nearRightCorner, color);
        var farLeft = new VertexPositionColor(farLeftCorner, color);
        var farRight = new VertexPositionColor(farRightCorner, color);
        VertexPositionColor[] quadVertices = new[] { nearLeft, farLeft, farRight, nearLeft, farRight, nearRight };
        return quadVertices;
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
            string goalMeshName = BoardSceneLayout.GetGoalMeshName(
                match.BoardSize,
                player.TurnOrder,
                match.Players.Count);
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
        float fieldOfView = MathHelper.ToRadians(FieldOfViewDegrees);
        var projection = Matrix.CreatePerspectiveFieldOfView(
            fieldOfView,
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

            bool hasGoalColor = _goalColorsByMeshName.TryGetValue(mesh.Name, out Color goalColor);
            if (hasGoalColor)
            {
                SceneModel.TintMesh(mesh, goalColor);
                boardModel.DrawMesh(mesh, Matrix.Identity, camera);
            }
        }
    }

    private void DrawReachableSquares(MatchViewModel match, SceneCamera camera)
    {
        IReadOnlyList<BoardCoordinates> reachableSquares = match.MatchPlay.ReachableSquares;
        if (reachableSquares.Count == 0)
        {
            return;
        }

        _graphicsDevice.BlendState = BlendState.NonPremultiplied;
        _graphicsDevice.DepthStencilState = DepthStencilState.None;
        _highlightEffect.View = camera.View;
        _highlightEffect.Projection = camera.Projection;
        foreach (BoardCoordinates square in reachableSquares)
        {
            DrawSquareHighlight(match.BoardSize, square);
        }

        ApplyDeviceStates();
    }

    private void DrawSquareHighlight(byte boardSize, BoardCoordinates square)
    {
        Vector3 center = BoardSceneLayout.GetCellCenter(boardSize, square);
        _highlightEffect.World = Matrix.CreateTranslation(center);
        foreach (EffectPass pass in _highlightEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _graphicsDevice.DrawUserPrimitives(
                PrimitiveType.TriangleList,
                _highlightVertices,
                FirstVertex,
                QuadTriangleCount);
        }
    }

    private void DrawWalls(MatchViewModel match, SceneCamera camera)
    {
        _wallEffect.View = camera.View;
        _wallEffect.Projection = camera.Projection;
        _wallEffect.Alpha = OpaqueAlpha;
        foreach (Wall wall in match.MatchPlay.Walls)
        {
            DrawWall(match.BoardSize, wall, Palette.Wall);
        }

        WallPlacementViewModel wallPlacement = match.WallPlacement;
        if (wallPlacement.PendingWall is Wall pendingWall)
        {
            DrawWall(match.BoardSize, pendingWall, Palette.PendingWall);
        }

        if (wallPlacement.HoveredWall is Wall hoveredWall)
        {
            DrawWallGhost(match.BoardSize, hoveredWall, wallPlacement.IsHoveredWallValid);
        }
    }

    private void DrawWallGhost(byte boardSize, Wall hoveredWall, bool isValid)
    {
        Color ghostColor = isValid ? Palette.ValidWallGhost : Palette.InvalidWallGhost;
        _graphicsDevice.BlendState = BlendState.NonPremultiplied;
        _graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
        _wallEffect.Alpha = GhostAlpha;
        DrawWall(boardSize, hoveredWall, ghostColor);
        _wallEffect.Alpha = OpaqueAlpha;
        ApplyDeviceStates();
    }

    private void DrawWall(byte boardSize, Wall wall, Color color)
    {
        _wallEffect.World = BoardSceneLayout.GetWallTransform(boardSize, wall);
        _wallEffect.DiffuseColor = color.ToVector3();
        WallMesh.Draw(_graphicsDevice, _wallEffect);
    }

    private void DrawPawn(MatchViewModel match, BoardPawn pawn, SceneCamera camera)
    {
        Vector3 pawnCenter = BoardSceneLayout.GetCellCenter(match.BoardSize, pawn.Coordinates);
        var world = Matrix.CreateTranslation(pawnCenter);
        Color pawnColor = GetPawnColor(match.MatchPlay, pawn);
        foreach (ModelMesh mesh in _pawnModel.Model.Meshes)
        {
            SceneModel.TintMesh(mesh, pawnColor);
            _pawnModel.DrawMesh(mesh, world, camera);
        }
    }

    private static Color GetPawnColor(MatchPlayViewModel matchPlayViewModel, BoardPawn pawn)
    {
        Color pawnColor = Palette.GetPawnColor(pawn.Symbol);
        if (pawn.Coordinates != matchPlayViewModel.SelectedPawn)
        {
            return pawnColor;
        }

        var selectedColor = Color.Lerp(pawnColor, Palette.White, SelectedPawnLightening);
        return selectedColor;
    }

    private Vector3 FindSurfacePoint(MatchViewModel match, Point cursorPosition)
    {
        SceneCamera camera = CreateCamera(match.OwnTurnOrder);
        Ray cursorRay = CreateCursorRay(cursorPosition, camera);
        float? surfaceDistance = cursorRay.Intersects(BoardSceneLayout.Surface);
        if (surfaceDistance is not float distance)
        {
            return BoardSceneLayout.OffBoardPoint;
        }

        Vector3 surfaceOffset = cursorRay.Direction * distance;
        return cursorRay.Position + surfaceOffset;
    }

    private Ray CreateCursorRay(Point cursorPosition, SceneCamera camera)
    {
        var nearScreenPoint = new Vector3(cursorPosition.X, cursorPosition.Y, NearScreenDepth);
        var farScreenPoint = new Vector3(cursorPosition.X, cursorPosition.Y, FarScreenDepth);
        Viewport viewport = _graphicsDevice.Viewport;
        Vector3 nearPoint = viewport.Unproject(nearScreenPoint, camera.Projection, camera.View, Matrix.Identity);
        Vector3 farPoint = viewport.Unproject(farScreenPoint, camera.Projection, camera.View, Matrix.Identity);
        Vector3 cursorDirection = farPoint - nearPoint;
        var normalizedDirection = Vector3.Normalize(cursorDirection);
        return new Ray(nearPoint, normalizedDirection);
    }
}
