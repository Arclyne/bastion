using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Bastion.Client.Screens.Matches.Scene;

public static class WallMesh
{
    public const int TriangleCount = 12;

    private const float HalfSide = 0.5f;

    private static readonly VertexPositionNormalTexture[] _vertices = CreateUnitBox();

    public static void Draw(GraphicsDevice graphicsDevice, BasicEffect effect)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(effect);

        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _vertices, 0, TriangleCount);
        }
    }

    private static VertexPositionNormalTexture[] CreateUnitBox()
    {
        var vertices = new List<VertexPositionNormalTexture>();
        AddFace(vertices, Vector3.Up, Vector3.Right, Vector3.Backward);
        AddFace(vertices, Vector3.Down, Vector3.Right, Vector3.Forward);
        AddFace(vertices, Vector3.Right, Vector3.Forward, Vector3.Up);
        AddFace(vertices, Vector3.Left, Vector3.Backward, Vector3.Up);
        AddFace(vertices, Vector3.Backward, Vector3.Right, Vector3.Up);
        AddFace(vertices, Vector3.Forward, Vector3.Left, Vector3.Up);
        return vertices.ToArray();
    }

    private static void AddFace(List<VertexPositionNormalTexture> vertices, Vector3 normal, Vector3 side, Vector3 up)
    {
        Vector3 center = normal * HalfSide;
        Vector3 sideOffset = side * HalfSide;
        Vector3 upOffset = up * HalfSide;
        Vector3 bottomLeft = center - sideOffset - upOffset;
        Vector3 bottomRight = center + sideOffset - upOffset;
        Vector3 topLeft = center - sideOffset + upOffset;
        Vector3 topRight = center + sideOffset + upOffset;
        vertices.Add(new VertexPositionNormalTexture(bottomLeft, normal, Vector2.Zero));
        vertices.Add(new VertexPositionNormalTexture(topLeft, normal, Vector2.Zero));
        vertices.Add(new VertexPositionNormalTexture(topRight, normal, Vector2.Zero));
        vertices.Add(new VertexPositionNormalTexture(bottomLeft, normal, Vector2.Zero));
        vertices.Add(new VertexPositionNormalTexture(topRight, normal, Vector2.Zero));
        vertices.Add(new VertexPositionNormalTexture(bottomRight, normal, Vector2.Zero));
    }
}
