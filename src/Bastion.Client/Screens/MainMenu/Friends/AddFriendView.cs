using System;
using Bastion.Client.Controls;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms;
using Gum.Forms.Controls;
using Gum.Wireframe;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class AddFriendView : StackPanel
{
    private const float ViewWidth = PanelMetrics.ContentWidth;
    private const float BackButtonWidth = 110;
    private const float ControlHeight = 40;
    private const float SearchButtonWidth = 96;
    private const float SearchFieldWidth = ViewWidth - SearchButtonWidth - PanelMetrics.SectionSpacing;
    private const float ResultsHeight = 380;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(AddFriendView));

    private readonly FriendsPanelViewModel _friendsPanelViewModel;

    public AddFriendView(FriendsPanelViewModel friendsPanelViewModel)
    {
        ArgumentNullException.ThrowIfNull(friendsPanelViewModel);

        _friendsPanelViewModel = friendsPanelViewModel;
        BindingContext = friendsPanelViewModel.AddFriend;
        Width = ViewWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Spacing = PanelMetrics.SectionSpacing;

        Button backButton = CreateBackButton();
        StackPanel searchRowStackPanel = CreateSearchRow();
        Label searchHintLabel = ViewFactory.CreateLabel(nameof(Strings.FriendsSearchHint), TextStyleCatalog.Hint);
        Label statusLabel = CreateStatus();
        ItemsControl resultsItemsControl = CreateResults();
        AddChild(backButton);
        AddChild(searchRowStackPanel);
        AddChild(searchHintLabel);
        AddChild(statusLabel);
        AddChild(resultsItemsControl);
    }

    private Button CreateBackButton()
    {
        Button backButton = ViewFactory.CreateButton(nameof(Strings.FriendsBack), BackButtonWidth, ControlHeight);
        ButtonSkin.CreateAndAttach(backButton, ButtonLookCatalog.Pill);
        backButton.Click += HandleBackClick;
        return backButton;
    }

    private void HandleBackClick(object? sender, EventArgs e)
    {
        _friendsPanelViewModel.CloseAddFriend();
    }

    private StackPanel CreateSearchRow()
    {
        var inputTextField = new TextField
        {
            Placeholder = nameof(Strings.FriendsSearchPlaceholder),
            Width = SearchFieldWidth,
            Height = ControlHeight,
        };
        inputTextField.SetBinding(nameof(TextField.Text), nameof(AddFriendViewModel.SearchText));

        Button searchButton = ViewFactory.CreateButton(nameof(Strings.FriendsSearch), SearchButtonWidth, ControlHeight);
        ButtonSkin.CreateAndAttach(searchButton, ButtonLookCatalog.Secondary);
        searchButton.SetBinding(nameof(IsEnabled), nameof(AddFriendViewModel.CanSearch));
        searchButton.Click += HandleSearchClick;

        var rowStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = PanelMetrics.SectionSpacing,
        };
        rowStackPanel.AddChild(inputTextField);
        rowStackPanel.AddChild(searchButton);
        return rowStackPanel;
    }

    private async void HandleSearchClick(object? sender, EventArgs e)
    {
        try
        {
            await _friendsPanelViewModel.AddFriend.SearchPlayers();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The player search could not run because the connection is closed.", ex);
        }
    }

    private static Label CreateStatus()
    {
        return ViewFactory.CreateBoundNotice(
            nameof(AddFriendViewModel.StatusText),
            nameof(AddFriendViewModel.HasStatus),
            TextStyleCatalog.FieldError);
    }

    private ItemsControl CreateResults()
    {
        ItemsControl resultsItemsControl = ViewFactory.CreateScrollList<ItemsControl>(ViewWidth, ResultsHeight);
        resultsItemsControl.VisualTemplate = new VisualTemplate(CreateResultRow);
        resultsItemsControl.Items = _friendsPanelViewModel.AddFriend.Results;
        return resultsItemsControl;
    }

    private GraphicalUiElement CreateResultRow(object? item)
    {
        FoundPlayerRowViewModel row = ViewFactory.RequireRow<FoundPlayerRowViewModel>(item);
        var foundPlayerRowView = new FoundPlayerRowView(_friendsPanelViewModel.AddFriend, row);
        return foundPlayerRowView.Visual;
    }
}
