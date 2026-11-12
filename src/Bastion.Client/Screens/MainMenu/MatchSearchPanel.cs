using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.Forms.Controls;
using log4net;

namespace Bastion.Client.Screens.MainMenu;

public sealed class MatchSearchPanel : DialogCard
{
    private const float ButtonWidth = 160;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(MatchSearchPanel));

    private readonly MatchSearchViewModel _viewModel;

    public MatchSearchPanel(MatchSearchViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;
        BindingContext = viewModel;
        SetBinding(nameof(IsVisible), nameof(MatchSearchViewModel.IsSearching));
        Content.AddChild(ViewFactory.CreateLabel(nameof(Strings.MatchSearchSearchingTitle), TextStyles.DialogTitle));
        Content.AddChild(ViewFactory.CreateBoundUntranslatedLabel(
            nameof(MatchSearchViewModel.SearchDetail),
            TextStyles.DialogMessage,
            DialogMetrics.ContentWidth));
        Content.AddChild(CreateCancelButton());
    }

    private Button CreateCancelButton()
    {
        Button cancelButton = ViewFactory.CreateButton(
            nameof(Strings.CommonCancelButton),
            ButtonWidth,
            DialogMetrics.ButtonHeight);
        ButtonSkin.CreateAndAttach(cancelButton, ButtonLooks.Secondary);
        cancelButton.SetBinding(nameof(IsEnabled), nameof(MatchSearchViewModel.CanCancelSearch));
        cancelButton.Click += HandleCancelClick;
        return cancelButton;
    }

    private async void HandleCancelClick(object? sender, EventArgs e)
    {
        try
        {
            await _viewModel.CancelSearch();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The match search could not be cancelled because the connection is closed.", ex);
        }
    }
}
