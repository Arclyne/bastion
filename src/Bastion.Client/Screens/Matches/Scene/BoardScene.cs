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
    private const float BoardSurfaceDistance = 0f;
    private const int OffBoardIndex = -1;
    private const float HighlightHalfSize = 0.4f;
    private const float HighlightHeight = 0.02f;
    private const int QuadVertexCount = 6;
    private const int QuadTriangleCount = 2;
    private const float SelectedPawnLightening = 0.35f;

    private static readonly Plane _boardSurface = new Plane(Vector3.Up, BoardSurfaceDistance);

    private readonly GraphicsDevice _graphicsDevice;
    private readonly SceneModel _classicBoardModel;
    private readonly SceneModel _quickBoardModel;
    private readonly SceneModel _pawnModel;
    private readonly BasicEffect _highlightEffect;
    private readonly VertexPositionColor[] _highlightVertices = new VertexPositionColor[QuadVertexCount];
    private readonly Dictionary<string, Color> _goalColorsByMeshName = new Dictionary<string, Color>();
    private MatchViewModel? _preparedMatch;
    private int _preparedPawnCount;

    public BoardScene(GraphicsDevice graphicsDevice, ContentManager content)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(content);

        _graphicsDevice = graphicsDevice;
        _classicBoardModel = new SceneModel(content.Load<Model>(ClassicBoardAssetName));
        _quickBoardModel = new SceneModel(content.Load<Model>(QuickBoardAssetName));
        _pawnModel = new SceneModel(content.Load<Model>(PawnAssetName));
        _highlightEffect = new BasicEffect(graphicsDevice)
        {
            VertexColorEnabled = true,
        };
    }

    public void Draw(MatchViewModel match)
    {
        ArgumentNullException.ThrowIfNull(match);

        PrepareGoalColors(match);
        SceneCamera camera = CreateCamera(match.OwnTurnOrder);
        ApplyDeviceStates();
        DrawBoard(GetBoardModel(match.BoardSize), camera);
        DrawReachableSquares(match, camera);
        foreach (BoardPawn pawn in match.Pawns)
        {
            DrawPawn(match, pawn, camera);
        }
    }

    public bool TryFindSquareUnderCursor(MatchViewModel match, Point cursorPosition, out BoardCoordinates square)
    {
        ArgumentNullException.ThrowIfNull(match);

        SceneCamera camera = CreateCamera(match.OwnTurnOrder);
        Ray cursorRay = CreateCursorRay(cursorPosition, camera);
        float? surfaceDistance = cursorRay.Intersects(_boardSurface);
        if (surfaceDistance is not float distance)
        {
            square = new BoardCoordinates(OffBoardIndex, OffBoardIndex);
            return false;
        }

        Vector3 surfaceOffset = cursorRay.Direction * distance;
        Vector3 surfacePoint = cursorRay.Position + surfaceOffset;
        return BoardSceneLayout.TryGetCoordinates(match.BoardSize, surfacePoint, out square);
    }

    public void Dispose()
    {
        _highlightEffect.Dispose();
    }

    private void PrepareGoalColors(MatchViewModel match)
    {
        if (ReferenceEquals(_preparedMatch, match) && _preparedPawnCount == match.Pawns.Count)
        {
            return;
        }

        _goalColorsByMeshName.Clear();
        foreach (MatchPlayerViewModel player in match.Players.Where(candidate => !candidate.HasLeft))
        {
            string goalMeshName =
                BoardSceneLayout.GetGoalMeshName(match.BoardSize, player.TurnOrder, match.Players.Count);
            _goalColorsByMeshName[goalMeshName] = Palette.GetPawnColor(player.PawnSymbol);
        }

        _preparedMatch = match;
        _preparedPawnCount = match.Pawns.Count;
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

    private void DrawReachableSquares(MatchViewModel match, SceneCamera camera)
    {
        IReadOnlyList<BoardCoordinates> reachableSquares = match.Play.ReachableSquares;
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
        float left = center.X - HighlightHalfSize;
        float right = center.X + HighlightHalfSize;
        float near = center.Z + HighlightHalfSize;
        float far = center.Z - HighlightHalfSize;
        var nearLeft = new Vector3(left, HighlightHeight, near);
        var nearRight = new Vector3(right, HighlightHeight, near);
        var farLeft = new Vector3(left, HighlightHeight, far);
        var farRight = new Vector3(right, HighlightHeight, far);
        Color color = Palette.ReachableSquare;
        _highlightVertices[0] = new VertexPositionColor(nearLeft, color);
        _highlightVertices[1] = new VertexPositionColor(farLeft, color);
        _highlightVertices[2] = new VertexPositionColor(farRight, color);
        _highlightVertices[3] = new VertexPositionColor(nearLeft, color);
        _highlightVertices[4] = new VertexPositionColor(farRight, color);
        _highlightVertices[5] = new VertexPositionColor(nearRight, color);
        foreach (EffectPass pass in _highlightEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _highlightVertices, 0, QuadTriangleCount);
        }
    }

    private void DrawPawn(MatchViewModel match, BoardPawn pawn, SceneCamera camera)
    {
        var world = Matrix.CreateTranslation(BoardSceneLayout.GetCellCenter(match.BoardSize, pawn.Coordinates));
        Color pawnColor = GetPawnColor(match.Play, pawn);
        foreach (ModelMesh mesh in _pawnModel.Model.Meshes)
        {
            SceneModel.TintMesh(mesh, pawnColor);
            _pawnModel.DrawMesh(mesh, world, camera);
        }
    }

    private static Color GetPawnColor(MatchPlayViewModel play, BoardPawn pawn)
    {
        Color pawnColor = Palette.GetPawnColor(pawn.Symbol);
        if (pawn.Coordinates != play.SelectedPawn)
        {
            return pawnColor;
        }

        var selectedColor = Color.Lerp(pawnColor, Palette.White, SelectedPawnLightening);
        return selectedColor;
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
