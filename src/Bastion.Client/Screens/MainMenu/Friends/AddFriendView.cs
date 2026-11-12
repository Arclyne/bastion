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
    private const float ViewSpacing = 10;
    private const float BackButtonWidth = 110;
    private const float ControlHeight = 40;
    private const float SearchButtonWidth = 96;
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
        Spacing = ViewSpacing;

        AddChild(CreateBackButton());
        AddChild(CreateSearchRow());
        AddChild(ViewFactory.CreateLabel(nameof(Strings.FriendsSearchHint), TextStyleCatalog.Hint));
        AddChild(CreateStatus());
        AddChild(CreateResults());
    }

    private Button CreateBackButton()
    {
        Button back = ViewFactory.CreateButton(nameof(Strings.FriendsBack), BackButtonWidth, ControlHeight);
        ButtonSkin.CreateAndAttach(back, ButtonLookCatalog.Pill);
        back.Click += HandleBackClick;
        return back;
    }

    private StackPanel CreateSearchRow()
    {
        var input = new TextField
        {
            Placeholder = nameof(Strings.FriendsSearchPlaceholder),
            Width = ViewWidth - SearchButtonWidth - ViewSpacing,
            Height = ControlHeight,
        };
        input.SetBinding(nameof(TextField.Text), nameof(AddFriendViewModel.SearchText));

        Button search = ViewFactory.CreateButton(nameof(Strings.FriendsSearch), SearchButtonWidth, ControlHeight);
        ButtonSkin.CreateAndAttach(search, ButtonLookCatalog.Secondary);
        search.SetBinding(nameof(IsEnabled), nameof(AddFriendViewModel.CanSearch));
        search.Click += HandleSearchClick;

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = ViewSpacing,
        };
        row.AddChild(input);
        row.AddChild(search);
        return row;
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
        ItemsControl results = ViewFactory.CreateScrollList<ItemsControl>(ViewWidth, ResultsHeight);
        results.VisualTemplate = new VisualTemplate(CreateResultRow);
        results.Items = _friendsPanelViewModel.AddFriend.Results;
        return results;
    }

    private GraphicalUiElement CreateResultRow(object? item)
    {
        FoundPlayerRowViewModel row = ViewFactory.RequireRow<FoundPlayerRowViewModel>(item);
        var rowView = new FoundPlayerRowView(_friendsPanelViewModel.AddFriend, row);
        return rowView.Visual;
    }

    private void HandleBackClick(object? sender, EventArgs e)
    {
        _friendsPanelViewModel.CloseAddFriend();
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
}
