using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using log4net;

namespace Bastion.Client.Screens.MainMenu;

public sealed class MatchSearchPanel : Panel
{
    private const float ButtonWidth = 160;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(MatchSearchPanel));

    private readonly MatchSearchViewModel _viewModel;

    public MatchSearchPanel(MatchSearchViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;
        BindingContext = viewModel;
        Width = DialogMetrics.Width;
        WidthUnits = DimensionUnitType.Absolute;
        Height = DialogMetrics.PaddingBottom;
        HeightUnits = DimensionUnitType.RelativeToChildren;
        SetBinding(nameof(IsVisible), nameof(MatchSearchViewModel.IsSearching));

        var content = new StackPanel
        {
            X = DialogMetrics.PaddingX,
            Y = DialogMetrics.PaddingTop,
            Spacing = DialogMetrics.SectionSpacing,
        };
        content.AddChild(ViewFactory.CreateLabel(nameof(Strings.MatchSearchSearchingTitle), TextStyles.DialogTitle));
        content.AddChild(ViewFactory.CreateBoundUntranslatedLabel(
            nameof(MatchSearchViewModel.SearchDetail),
            TextStyles.DialogMessage,
            DialogMetrics.ContentWidth));
        content.AddChild(CreateCancelButton());
        AddChild(content);
        Skins.ApplyCard(Visual);
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
