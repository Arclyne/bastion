using System;
using System.Collections.Generic;
using System.IO;
using Gum.Wireframe;
using KernSmith.Gum;
using Microsoft.Xna.Framework;
using MonoGameAndGum.Renderables;

namespace Bastion.Client.Theme;

public static class ThemeInitializer
{
    private const string FontsDirectory = "Fonts";
    private const int SingleFaceIndex = 0;

    private static readonly Dictionary<string, string> _fontFiles = new Dictionary<string, string>
    {
        [FontFamilies.SemiBold] = "Nunito-SemiBold.woff",
        [FontFamilies.Bold] = "Nunito-Bold.woff",
        [FontFamilies.ExtraBold] = "Nunito-ExtraBold.woff",
        [FontFamilies.Display] = "Fredoka-Bold.woff",
    };

    public static void Initialize(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);

        string fontsPath = Path.Combine(AppContext.BaseDirectory, game.Content.RootDirectory, FontsDirectory);
        foreach (KeyValuePair<string, string> fontFile in _fontFiles)
        {
            byte[] fontData = File.ReadAllBytes(Path.Combine(fontsPath, fontFile.Value));
            KernSmithFontCreator.RegisterFont(fontFile.Key, fontData, null, SingleFaceIndex);
        }

        CustomSetPropertyOnRenderable.InMemoryFontCreator = new KernSmithFontCreator(game.GraphicsDevice);
        ShapeRenderer.Self.Initialize(game.GraphicsDevice, game.Content);
    }
}
