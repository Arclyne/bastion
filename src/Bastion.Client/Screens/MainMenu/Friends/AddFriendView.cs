using System;
using System.Collections.Specialized;
using Bastion.Client.Controls;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
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
    private readonly ScrollViewer _results;

    public AddFriendView(FriendsPanelViewModel friendsPanelViewModel)
    {
        ArgumentNullException.ThrowIfNull(friendsPanelViewModel);

        _friendsPanelViewModel = friendsPanelViewModel;
        BindingContext = friendsPanelViewModel.AddFriend;
        Width = ViewWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Spacing = ViewSpacing;

        _results = ViewFactory.CreateScrollList(ViewWidth, ResultsHeight);

        AddChild(CreateBackButton());
        AddChild(CreateSearchRow());
        AddChild(ViewFactory.CreateLabel(nameof(Strings.FriendsSearchHint), TextStyles.Hint));
        AddChild(CreateStatus());
        AddChild(_results);

        friendsPanelViewModel.AddFriend.Results.CollectionChanged += HandleResultsChanged;
    }

    private Button CreateBackButton()
    {
        Button back = ViewFactory.CreateButton(nameof(Strings.FriendsBack), BackButtonWidth, ControlHeight);
        ButtonSkin.CreateAndAttach(back, ButtonLooks.Pill);
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
        ButtonSkin.CreateAndAttach(search, ButtonLooks.Secondary);
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
            TextStyles.FieldError);
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

    private void HandleResultsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ViewFactory.ClearRows(_results.InnerPanel);
        foreach (FoundPlayerRowViewModel row in _friendsPanelViewModel.AddFriend.Results)
        {
            var rowView = new FoundPlayerRowView(_friendsPanelViewModel.AddFriend, row);
            _results.InnerPanel.AddChild(rowView.Visual);
        }
    }
}
