using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Bastion.Client.Theme;

public static class IconCatalog
{
    private const string IconsDirectory = "Icons";
    private const string LostConnectionFileName = "lost_connection.png";
    private const string RejoinFileName = "rejoin.png";

    private static Texture2D? _lostConnection;
    private static Texture2D? _rejoin;

    public static Texture2D LostConnection =>
        _lostConnection ?? throw new InvalidOperationException("The icons were not loaded.");

    public static Texture2D Rejoin => _rejoin ?? throw new InvalidOperationException("The icons were not loaded.");

    public static void Initialize(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);

        string iconsPath = Path.Combine(AppContext.BaseDirectory, game.Content.RootDirectory, IconsDirectory);
        _lostConnection = Texture2D.FromFile(game.GraphicsDevice, Path.Combine(iconsPath, LostConnectionFileName));
        _rejoin = Texture2D.FromFile(game.GraphicsDevice, Path.Combine(iconsPath, RejoinFileName));
    }
}
