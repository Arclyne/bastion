using System;
using Bastion.Client.Localization;
using Bastion.Client.Networking;
using Bastion.Client.Platform;
using Bastion.Client.Screens;
using Bastion.Client.Screens.Matches;
using Bastion.Client.Screens.Matches.Scene;
using Bastion.Client.Theme;
using Gum;
using Gum.Forms;
using Gum.Forms.Input;
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
