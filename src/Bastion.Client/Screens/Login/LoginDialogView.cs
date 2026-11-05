using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.GueDeriving;
using log4net;

namespace Bastion.Client.Screens.Login;

public sealed class LoginDialogView : Panel
{
    private const float ButtonSpacing = 12;
    private const float Radius = 22;
    private const float BorderWidth = 1.5f;
    private const float ShadowOffsetY = 30;
    private const float ShadowBlur = 60;
    private const int ButtonsPerRow = 2;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(LoginDialogView));

    private readonly LoginViewModel _viewModel;

    public LoginDialogView(LoginViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;
        Anchor(Gum.Wireframe.Anchor.Center);
        Width = DialogMetrics.Width;
        WidthUnits = DimensionUnitType.Absolute;
        Height = DialogMetrics.PaddingBottom;
        HeightUnits = DimensionUnitType.RelativeToChildren;
        AddChild(CreateContent());
        Shapes.InsertBehindContent(Visual, CreateSurface());
    }

    private StackPanel CreateContent()
    {
        var content = new StackPanel
        {
            X = DialogMetrics.PaddingX,
            Y = DialogMetrics.PaddingTop,
            Spacing = DialogMetrics.SectionSpacing,
        };
        content.AddChild(ViewFactory.CreateBoundLabel(
            nameof(LoginViewModel.DialogTitleKey),
            TextStyles.DialogTitle,
            DialogMetrics.ContentWidth));
        content.AddChild(ViewFactory.CreateBoundLabel(
            nameof(LoginViewModel.DialogMessageKey),
            TextStyles.DialogMessage,
            DialogMetrics.ContentWidth));
        content.AddChild(CreateButtons(DialogMetrics.ContentWidth));
        return content;
    }

    private StackPanel CreateButtons(float contentWidth)
    {
        float buttonWidth = (contentWidth - ButtonSpacing) / ButtonsPerRow;

        Button retryButton = CreateButton(nameof(Strings.CommonRetryButton), buttonWidth, ButtonLooks.Primary);
        retryButton.SetBinding(nameof(IsVisible), nameof(LoginViewModel.CanRetry));
        retryButton.Click += HandleRetryClick;

        Button acceptButton = CreateButton(nameof(Strings.CommonAcceptButton), buttonWidth, ButtonLooks.Secondary);
        acceptButton.Click += HandleAcceptClick;

        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = ButtonSpacing };
        row.AddChild(retryButton);
        row.AddChild(acceptButton);
        return row;
    }

    private static Button CreateButton(string textKey, float width, ButtonLook look)
    {
        var button = new Button
        {
            Text = textKey,
            Width = width,
            Height = DialogMetrics.ButtonHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        ButtonSkin.CreateAndAttach(button, look);
        return button;
    }

    private static RectangleRuntime CreateSurface()
    {
        RectangleRuntime surface = Shapes.CreateRounded(Palette.Card, Radius);
        surface.StrokeColor = Palette.Border;
        surface.StrokeWidth = BorderWidth;
        surface.HasDropshadow = true;
        surface.DropshadowColor = Palette.Shadow;
        surface.DropshadowOffsetY = ShadowOffsetY;
        surface.DropshadowBlur = ShadowBlur;
        return surface;
    }

    private async void HandleRetryClick(object? sender, EventArgs e)
    {
        try
        {
            await _viewModel.RetryLogIn();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The sign-in retry could not be completed because the connection is closed.", ex);
        }
    }

    private void HandleAcceptClick(object? sender, EventArgs e)
    {
        _viewModel.CloseDialog();
    }
}
