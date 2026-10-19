using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
using Gum.GueDeriving;

namespace Bastion.Client.Screens.Login;

public sealed class LoginDialogView : Panel
{
    private const float DialogWidth = 420;
    private const float PaddingX = 28;
    private const float PaddingTop = 26;
    private const float PaddingBottom = 24;
    private const float SectionSpacing = 14;
    private const float ButtonSpacing = 12;
    private const float ButtonHeight = 46;
    private const float Radius = 22;
    private const float BorderWidth = 1.5f;
    private const float ShadowOffsetY = 30;
    private const float ShadowBlur = 60;

    private readonly LoginViewModel _viewModel;

    public LoginDialogView(LoginViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;
        Anchor(Gum.Wireframe.Anchor.Center);
        Width = DialogWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Height = PaddingBottom;
        HeightUnits = DimensionUnitType.RelativeToChildren;
        AddChild(CreateContent());
        Shapes.InsertBehindContent(Visual, CreateSurface());
    }

    private StackPanel CreateContent()
    {
        float contentWidth = DialogWidth - (PaddingX * 2);
        var content = new StackPanel { X = PaddingX, Y = PaddingTop, Spacing = SectionSpacing };
        content.AddChild(CreateBoundLabel(nameof(LoginViewModel.DialogTitleKey), TextStyles.DialogTitle, contentWidth));
        content.AddChild(CreateBoundLabel(
            nameof(LoginViewModel.DialogMessageKey),
            TextStyles.DialogMessage,
            contentWidth));
        content.AddChild(CreateButtons(contentWidth));
        return content;
    }

    private StackPanel CreateButtons(float contentWidth)
    {
        float buttonWidth = (contentWidth - ButtonSpacing) / 2;

        Button retryButton = CreateButton(nameof(Strings.CommonRetryButton), buttonWidth, ButtonLooks.Primary);
        retryButton.SetBinding(nameof(IsVisible), nameof(LoginViewModel.CanRetry));
        retryButton.Click += OnRetryClick;

        Button acceptButton = CreateButton(nameof(Strings.CommonAcceptButton), buttonWidth, ButtonLooks.Secondary);
        acceptButton.Click += OnAcceptClick;

        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = ButtonSpacing };
        row.AddChild(retryButton);
        row.AddChild(acceptButton);
        return row;
    }

    private static Label CreateBoundLabel(string viewModelProperty, TextStyle style, float width)
    {
        var label = new Label { Width = width, WidthUnits = DimensionUnitType.Absolute };
        style.ApplyTo((LabelVisual)label.Visual);
        label.SetBinding(nameof(Label.Text), viewModelProperty);
        return label;
    }

    private static Button CreateButton(string textKey, float width, ButtonLook look)
    {
        var button = new Button
        {
            Text = textKey,
            Width = width,
            Height = ButtonHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        ButtonSkin.Apply(button, look);
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

    private async void OnRetryClick(object? sender, EventArgs e)
    {
        await _viewModel.Retry();
    }

    private void OnAcceptClick(object? sender, EventArgs e)
    {
        _viewModel.CloseDialog();
    }
}
