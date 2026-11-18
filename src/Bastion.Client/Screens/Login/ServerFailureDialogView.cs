using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.Forms.Controls;
using log4net;

namespace Bastion.Client.Screens.Login;

public sealed class ServerFailureDialogView : DialogCard
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(ServerFailureDialogView));

    private readonly LoginViewModel _viewModel;

    public ServerFailureDialogView(LoginViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;
        Anchor(Gum.Wireframe.Anchor.Center);
        Label titleLabel = ViewFactory.CreateBoundLabel(
            nameof(LoginViewModel.DialogTitleKey),
            TextStyleCatalog.DialogTitle,
            DialogMetrics.ContentWidth);
        Label messageLabel = ViewFactory.CreateBoundLabel(
            nameof(LoginViewModel.DialogMessageKey),
            TextStyleCatalog.DialogMessage,
            DialogMetrics.ContentWidth);
        StackPanel buttonsStackPanel = CreateButtons();
        Content.AddChild(titleLabel);
        Content.AddChild(messageLabel);
        Content.AddChild(buttonsStackPanel);
    }

    private StackPanel CreateButtons()
    {
        Button retryButton = CreateDialogButton(nameof(Strings.CommonRetryButton), ButtonLookCatalog.Primary);
        retryButton.SetBinding(nameof(IsVisible), nameof(LoginViewModel.CanRetry));
        retryButton.Click += HandleRetryClick;

        Button acceptButton = CreateDialogButton(nameof(Strings.CommonAcceptButton), ButtonLookCatalog.Secondary);
        acceptButton.Click += HandleAcceptClick;

        var rowStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = DialogMetrics.ButtonSpacing,
        };
        rowStackPanel.AddChild(retryButton);
        rowStackPanel.AddChild(acceptButton);
        return rowStackPanel;
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
