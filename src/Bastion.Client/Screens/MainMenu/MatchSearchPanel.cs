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
        SetBinding(nameof(IsVisible), nameof(MatchSearchViewModel.IsPanelVisible));

        var content = new StackPanel
        {
            X = DialogMetrics.PaddingX,
            Y = DialogMetrics.PaddingTop,
            Spacing = DialogMetrics.SectionSpacing,
        };
        content.AddChild(CreateSearchingSection());
        content.AddChild(CreateMatchFoundSection());
        AddChild(content);
        Skins.ApplyCard(Visual);
    }

    private static StackPanel CreateSection(string titleKey, string detailProperty, Button button)
    {
        var section = new StackPanel
        {
            Spacing = DialogMetrics.SectionSpacing,
        };
        section.AddChild(ViewFactory.CreateLabel(titleKey, TextStyles.DialogTitle));
        section.AddChild(ViewFactory.CreateBoundUntranslatedLabel(
            detailProperty,
            TextStyles.DialogMessage,
            DialogMetrics.ContentWidth));
        section.AddChild(button);
        return section;
    }

    private static Button CreateButton(string textKey, ButtonLook look)
    {
        Button button = ViewFactory.CreateButton(textKey, ButtonWidth, DialogMetrics.ButtonHeight);
        ButtonSkin.CreateAndAttach(button, look);
        return button;
    }

    private StackPanel CreateSearchingSection()
    {
        Button cancelButton = CreateButton(nameof(Strings.CommonCancelButton), ButtonLooks.Secondary);
        cancelButton.SetBinding(nameof(IsEnabled), nameof(MatchSearchViewModel.CanCancelSearch));
        cancelButton.Click += HandleCancelClick;

        StackPanel section = CreateSection(
            nameof(Strings.MatchSearchSearchingTitle),
            nameof(MatchSearchViewModel.SearchDetail),
            cancelButton);
        section.SetBinding(nameof(IsVisible), nameof(MatchSearchViewModel.IsSearching));
        return section;
    }

    private StackPanel CreateMatchFoundSection()
    {
        Button acceptButton = CreateButton(nameof(Strings.CommonAcceptButton), ButtonLooks.Primary);
        acceptButton.Click += HandleAcceptClick;

        StackPanel section = CreateSection(
            nameof(Strings.MatchSearchFoundTitle),
            nameof(MatchSearchViewModel.RivalText),
            acceptButton);
        section.SetBinding(nameof(IsVisible), nameof(MatchSearchViewModel.IsMatchFound));
        return section;
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

    private void HandleAcceptClick(object? sender, EventArgs e)
    {
        _viewModel.DismissMatchFound();
    }
}
