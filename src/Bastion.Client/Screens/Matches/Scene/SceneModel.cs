using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Bastion.Client.Screens.Matches.Scene;

public sealed class SceneModel
{
    private const float DisplayGamma = 2.2f;

    private readonly Matrix[] _absoluteBoneTransforms;

    public SceneModel(Model model)
    {
        ArgumentNullException.ThrowIfNull(model);

        Model = model;
        _absoluteBoneTransforms = new Matrix[model.Bones.Count];
        model.CopyAbsoluteBoneTransformsTo(_absoluteBoneTransforms);
        PrepareEffects(model);
    }

    public Model Model { get; }

    public static void TintMesh(ModelMesh mesh, Color color)
    {
        ArgumentNullException.ThrowIfNull(mesh);

        foreach (BasicEffect effect in mesh.Effects)
        {
            effect.DiffuseColor = color.ToVector3();
        }
    }

    public void DrawMesh(ModelMesh mesh, Matrix world, SceneCamera camera)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentNullException.ThrowIfNull(camera);

        Matrix meshWorld = _absoluteBoneTransforms[mesh.ParentBone.Index] * world;
        foreach (BasicEffect effect in mesh.Effects)
        {
            effect.World = meshWorld;
            effect.View = camera.View;
            effect.Projection = camera.Projection;
        }

        mesh.Draw();
    }

    private static void PrepareEffects(Model model)
    {
        var preparedEffects = new HashSet<BasicEffect>();
        foreach (ModelMesh mesh in model.Meshes)
        {
            foreach (BasicEffect effect in mesh.Effects)
            {
                PrepareEffect(effect, preparedEffects);
            }
        }
    }

    private static void PrepareEffect(BasicEffect effect, HashSet<BasicEffect> preparedEffects)
    {
        if (!preparedEffects.Add(effect))
        {
            return;
        }

        effect.EnableDefaultLighting();
        effect.PreferPerPixelLighting = true;
        effect.DiffuseColor = ConvertLinearToDisplay(effect.DiffuseColor);
    }

    private static Vector3 ConvertLinearToDisplay(Vector3 linearColor)
    {
        float inverseGamma = 1f / DisplayGamma;
        float red = MathF.Pow(linearColor.X, inverseGamma);
        float green = MathF.Pow(linearColor.Y, inverseGamma);
        float blue = MathF.Pow(linearColor.Z, inverseGamma);
        return new Vector3(red, green, blue);
    }
}
