using System;
using Bastion.Client.Localization;
using Bastion.Client.Networking;
using Bastion.Client.Platform;
using Bastion.Client.Screens;
using Bastion.Client.Screens.Matches;
using Bastion.Client.Screens.Matches.Scene;
using Bastion.Client.Theme;
using Bastion.Domain.Matches;
using Gum;
using Gum.Forms;
using Gum.Forms.Controls;
using Gum.Forms.Input;
using Gum.Wireframe;
using Microsoft.Xna.Framework;

namespace Bastion.Client;

public sealed class BastionGame : Game
{
    private const string ContentRootDirectory = "Content";
    private const int InitialWindowWidth = 1280;
    private const int InitialWindowHeight = 720;
    private const int MinimumWindowWidth = 1024;
    private const int MinimumWindowHeight = 600;
    private const float ReferenceUiHeight = 768f;
    private const float MinimumUiScale = 0.8f;
    private const float MaximumUiScale = 1.5f;

    private readonly GraphicsDeviceManager _graphics;
    private readonly ServerConnection _serverConnection;
    private ScreenNavigator? _screenNavigator;
    private BoardScene? _boardScene;

    public BastionGame()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = InitialWindowWidth,
            PreferredBackBufferHeight = InitialWindowHeight,
        };
        _serverConnection = new ServerConnection(TimeProvider.System);
        Content.RootDirectory = ContentRootDirectory;
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
    }

    protected override void Initialize()
    {
        GumService.Default.Initialize(this, DefaultVisualsVersion.V3);
        GumService.Default.UseSingleThreadedAsync();
        ThemeInitializer.Initialize(this);
        var languagePreferenceStore = new LanguagePreferenceStore();
        var languages = new LanguageService(languagePreferenceStore, GumService.Default.LocalizationService);
        languages.LoadInitialLanguage();
        var clipboard = new GumClipboard(GumService.Default.Clipboard);
        var localServices = new LocalServices(languages, clipboard, TimeProvider.System);
        var screenHost = new GumScreenHost(GumService.Default.Root, GumService.Default.ModalRoot);
        _screenNavigator = new ScreenNavigator(_serverConnection, localServices, screenHost);
        _screenNavigator.ShowLogin();
        Window.ClientSizeChanged += HandleClientSizeChanged;
        ApplyUiScale();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _boardScene = new BoardScene(GraphicsDevice, Content);
        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        GumService.Default.Update(gameTime);
        if (GumService.Default.Keyboard.KeyPushed(Keys.Escape))
        {
            _screenNavigator?.ReceiveEscapeKey();
        }

        ForwardBoardInput();
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Palette.Background);
        DrawShownMatchBoard();
        GumService.Default.Draw();
        base.Draw(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Window.ClientSizeChanged -= HandleClientSizeChanged;
            _screenNavigator?.Dispose();
            _boardScene?.Dispose();
            _serverConnection.Dispose();
        }

        base.Dispose(disposing);
    }

    private void HandleClientSizeChanged(object? sender, EventArgs e)
    {
        KeepMinimumWindowSize();
        ApplyUiScale();
    }

    private void KeepMinimumWindowSize()
    {
        int width = Math.Max(Window.ClientBounds.Width, MinimumWindowWidth);
        int height = Math.Max(Window.ClientBounds.Height, MinimumWindowHeight);
        bool isSameSize = width == _graphics.PreferredBackBufferWidth && height == _graphics.PreferredBackBufferHeight;
        if (isSameSize)
        {
            return;
        }

        _graphics.PreferredBackBufferWidth = width;
        _graphics.PreferredBackBufferHeight = height;
        _graphics.ApplyChanges();
    }

    private void ApplyUiScale()
    {
        int windowWidth = GraphicsDevice.Viewport.Width;
        int windowHeight = GraphicsDevice.Viewport.Height;
        float uiScale = Math.Clamp(windowHeight / ReferenceUiHeight, MinimumUiScale, MaximumUiScale);

        GumService.Default.Renderer.Camera.Zoom = uiScale;
        GumService.Default.CanvasWidth = windowWidth / uiScale;
        GumService.Default.CanvasHeight = windowHeight / uiScale;
        GumService.Default.Root.UpdateLayout();
    }

    private void ForwardBoardInput()
    {
        MatchViewModel? shownMatch = _screenNavigator?.ShownMatch;
        if (shownMatch is null || _boardScene is null)
        {
            return;
        }

        ForwardWallKeys(shownMatch.WallPlacement);
        ICursor cursor = GumService.Default.Cursor;
        if (shownMatch.WallPlacement.IsActive)
        {
            ForwardWallCursor(_boardScene, shownMatch, cursor);
            return;
        }

        ForwardBoardClick(_boardScene, shownMatch, cursor);
    }

    private static void ForwardWallKeys(WallPlacementViewModel wallPlacement)
    {
        if (InteractiveGue.CurrentInputReceiver is not null)
        {
            return;
        }

        if (GumService.Default.Keyboard.KeyPushed(Keys.R))
        {
            wallPlacement.Rotate();
        }

        if (GumService.Default.Keyboard.KeyPushed(Keys.Enter))
        {
            _ = wallPlacement.Confirm();
        }

        if (GumService.Default.Keyboard.KeyPushed(Keys.Escape))
        {
            wallPlacement.Cancel();
        }
    }

    private static void ForwardWallCursor(BoardScene boardScene, MatchViewModel shownMatch, ICursor cursor)
    {
        WallPlacementViewModel wallPlacement = shownMatch.WallPlacement;
        if (cursor.SecondaryClick)
        {
            wallPlacement.Rotate();
        }

        var cursorPosition = new Point(cursor.X, cursor.Y);
        if (IsOverGumControl(cursor)
            || !boardScene.TryFindGrooveUnderCursor(shownMatch, cursorPosition, out BoardCoordinates groove))
        {
            wallPlacement.ClearHover();
            return;
        }

        wallPlacement.HoverGroove(groove);
        if (cursor.PrimaryClick)
        {
            wallPlacement.ChooseGroove(groove);
        }
    }

    private static void ForwardBoardClick(BoardScene boardScene, MatchViewModel shownMatch, ICursor cursor)
    {
        if (!cursor.PrimaryClick || IsOverGumControl(cursor))
        {
            return;
        }

        var cursorPosition = new Point(cursor.X, cursor.Y);
        if (!boardScene.TryFindSquareUnderCursor(shownMatch, cursorPosition, out BoardCoordinates square))
        {
            shownMatch.Play.ClearSelection();
            return;
        }

        _ = shownMatch.Play.SelectSquare(square);
    }

    private static bool IsOverGumControl(ICursor cursor)
    {
        object? controlOver = cursor.VisualOver?.FormsControlAsObject;
        return controlOver is FrameworkElement && controlOver is not ScreenView;
    }

    private void DrawShownMatchBoard()
    {
        MatchViewModel? shownMatch = _screenNavigator?.ShownMatch;
        if (shownMatch is null || _boardScene is null)
        {
            return;
        }

        _boardScene.Draw(shownMatch);
    }
}
