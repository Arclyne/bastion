using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Bastion.Client.Screens.Matches.Scene;

public static class WallMesh
{
    public const float HalfSide = 0.5f;
    private const int FirstVertex = 0;
    private const int VerticesPerTriangle = 3;

    private static readonly VertexPositionNormalTexture[] _vertices = CreateUnitBox();
    private static readonly int _triangleCount = _vertices.Length / VerticesPerTriangle;

    public static void Draw(GraphicsDevice graphicsDevice, BasicEffect effect)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(effect);

        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _vertices, FirstVertex, _triangleCount);
        }
    }

    private static VertexPositionNormalTexture[] CreateUnitBox()
    {
        var topFace = new BoxFace(Vector3.Up, Vector3.Right, Vector3.Backward);
        var bottomFace = new BoxFace(Vector3.Down, Vector3.Right, Vector3.Forward);
        var rightFace = new BoxFace(Vector3.Right, Vector3.Forward, Vector3.Up);
        var leftFace = new BoxFace(Vector3.Left, Vector3.Backward, Vector3.Up);
        var backFace = new BoxFace(Vector3.Backward, Vector3.Right, Vector3.Up);
        var frontFace = new BoxFace(Vector3.Forward, Vector3.Left, Vector3.Up);

        var vertices = new List<VertexPositionNormalTexture>();
        AddFace(vertices, topFace);
        AddFace(vertices, bottomFace);
        AddFace(vertices, rightFace);
        AddFace(vertices, leftFace);
        AddFace(vertices, backFace);
        AddFace(vertices, frontFace);
        return vertices.ToArray();
    }

    private static void AddFace(List<VertexPositionNormalTexture> vertices, BoxFace face)
    {
        Vector3 center = face.Normal * HalfSide;
        Vector3 sideOffset = face.Side * HalfSide;
        Vector3 upOffset = face.Up * HalfSide;
        Vector3 bottomLeft = center - sideOffset - upOffset;
        Vector3 bottomRight = center + sideOffset - upOffset;
        Vector3 topLeft = center - sideOffset + upOffset;
        Vector3 topRight = center + sideOffset + upOffset;
        var bottomLeftVertex = new VertexPositionNormalTexture(bottomLeft, face.Normal, Vector2.Zero);
        var bottomRightVertex = new VertexPositionNormalTexture(bottomRight, face.Normal, Vector2.Zero);
        var topLeftVertex = new VertexPositionNormalTexture(topLeft, face.Normal, Vector2.Zero);
        var topRightVertex = new VertexPositionNormalTexture(topRight, face.Normal, Vector2.Zero);
        vertices.Add(bottomLeftVertex);
        vertices.Add(topLeftVertex);
        vertices.Add(topRightVertex);
        vertices.Add(bottomLeftVertex);
        vertices.Add(topRightVertex);
        vertices.Add(bottomRightVertex);
    }
}
