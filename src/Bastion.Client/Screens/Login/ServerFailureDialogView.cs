using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using log4net;

namespace Bastion.Client.Screens.Login;

public sealed class ServerFailureDialogView : Panel
{
    private const float ButtonSpacing = 12;
    private const int ButtonsPerRow = 2;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(ServerFailureDialogView));

    private readonly LoginViewModel _viewModel;

    public ServerFailureDialogView(LoginViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;
        Anchor(Gum.Wireframe.Anchor.Center);
        Width = DialogMetrics.Width;
        WidthUnits = DimensionUnitType.Absolute;
        Height = DialogMetrics.PaddingBottom;
        HeightUnits = DimensionUnitType.RelativeToChildren;
        AddChild(CreateContent());
        Skins.ApplyCard(Visual);
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
        content.AddChild(CreateButtons());
        return content;
    }

    private StackPanel CreateButtons()
    {
        float buttonWidth = (DialogMetrics.ContentWidth - ButtonSpacing) / ButtonsPerRow;

        Button retryButton = CreateButton(nameof(Strings.CommonRetryButton), buttonWidth, ButtonLooks.Primary);
        retryButton.SetBinding(nameof(IsVisible), nameof(LoginViewModel.CanRetry));
        retryButton.Click += HandleRetryClick;

        Button acceptButton = CreateButton(nameof(Strings.CommonAcceptButton), buttonWidth, ButtonLooks.Secondary);
        acceptButton.Click += HandleAcceptClick;

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = ButtonSpacing,
        };
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

    private async void HandleRetryClick(object? sender, EventArgs e)
    {
        try
        {
            await _viewModel.RetryLogIn();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The login retry could not be completed because the connection is closed.", ex);
        }
    }

    private void HandleAcceptClick(object? sender, EventArgs e)
    {
        _viewModel.CloseDialog();
    }
}
