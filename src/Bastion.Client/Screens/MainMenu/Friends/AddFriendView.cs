using System;
using System.Collections.Specialized;
using Bastion.Client.Controls;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class AddFriendView : StackPanel
{
    private const float ViewWidth = 360;
    private const float ViewSpacing = 10;
    private const float BackButtonWidth = 110;
    private const float ControlHeight = 40;
    private const float SearchButtonWidth = 96;
    private const float ResultsHeight = 380;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(AddFriendView));

    private readonly FriendsPanelViewModel _panel;
    private readonly ScrollViewer _results;

    public AddFriendView(FriendsPanelViewModel panel)
    {
        ArgumentNullException.ThrowIfNull(panel);

        _panel = panel;
        BindingContext = panel.AddFriend;
        Width = ViewWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Spacing = ViewSpacing;

        _results = new ScrollViewer
        {
            Width = ViewWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = ResultsHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        Shapes.HideDefaultParts(_results);

        AddChild(CreateBackButton());
        AddChild(CreateSearchRow());
        AddChild(ViewFactory.CreateLabel(nameof(Strings.FriendsSearchHint), TextStyles.Hint));
        AddChild(CreateStatus());
        AddChild(_results);

        panel.AddFriend.Results.CollectionChanged += HandleResultsChanged;
    }

    private static Label CreateStatus()
    {
        Label status = ViewFactory.CreateLabel(string.Empty, TextStyles.FieldError);
        ((LabelVisual)status.Visual).LocalizeText = false;
        status.SetBinding(nameof(Label.Text), nameof(AddFriendViewModel.StatusText));
        status.SetBinding(nameof(IsVisible), nameof(AddFriendViewModel.HasStatus));
        return status;
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

        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = ViewSpacing };
        row.AddChild(input);
        row.AddChild(search);
        return row;
    }

    private void HandleResultsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        while (_results.InnerPanel.Children.Count > 0)
        {
            _results.InnerPanel.Children.RemoveAt(_results.InnerPanel.Children.Count - 1);
        }

        foreach (FoundPlayerRowViewModel row in _panel.AddFriend.Results)
        {
            _results.InnerPanel.AddChild(new FoundPlayerRowView(_panel.AddFriend, row).Visual);
        }
    }

    private void HandleBackClick(object? sender, EventArgs e)
    {
        _panel.CloseAddFriend();
    }

    private async void HandleSearchClick(object? sender, EventArgs e)
    {
        try
        {
            await _panel.AddFriend.SearchPlayers();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The player search could not run because the connection is closed.", ex);
        }
    }
}
