using System;
using Gum;
using Gum.Forms;
using Microsoft.Xna.Framework;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Screens;
using Bastion.Client.Theme;

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
    private readonly AccountClient _accountClient = new AccountClient();

    public BastionGame()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = InitialWindowWidth,
            PreferredBackBufferHeight = InitialWindowHeight,
        };
        Content.RootDirectory = ContentRootDirectory;
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
    }

    protected override void Initialize()
    {
        GumService.Default.Initialize(this, DefaultVisualsVersion.V3);
        GumService.Default.UseSingleThreadedAsync();
        ThemeInitializer.Initialize(this);
        var languages = new LanguageService(new LanguagePreferenceStore(), GumService.Default.LocalizationService);
        languages.Start();
        new ScreenNavigator(languages, _accountClient).ShowLogin();
        Window.ClientSizeChanged += OnClientSizeChanged;
        ApplyUiScale();
        base.Initialize();
    }

    protected override void Update(GameTime gameTime)
    {
        GumService.Default.Update(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Palette.Background);
        GumService.Default.Draw();
        base.Draw(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Window.ClientSizeChanged -= OnClientSizeChanged;
            _accountClient.Dispose();
        }

        base.Dispose(disposing);
    }

    private void OnClientSizeChanged(object? sender, EventArgs e)
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
}
