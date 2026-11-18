using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Bastion.Client.Controls;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class FriendListView : StackPanel
{
    private const float ListWidth = PanelMetrics.ContentWidth;
    private const float FilterHeight = 44;
    private const float AddFriendButtonWidth = 150;
    private const float ScrollHeight = 460;
    private const float FilterFieldWidth = ListWidth - AddFriendButtonWidth - PanelMetrics.SectionSpacing;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(FriendListView));

    private readonly FriendsPanelViewModel _friendsPanelViewModel;
    private readonly StackPanel _connectedRows;
    private readonly StackPanel _offlineRows;

    public FriendListView(FriendsPanelViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _friendsPanelViewModel = viewModel;
        _connectedRows = new StackPanel();
        _offlineRows = new StackPanel();
        Width = ListWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Spacing = PanelMetrics.SectionSpacing;

        StackPanel filterRow = CreateFilterRow();
        ScrollViewer groups = CreateGroups();
        Label emptyLabel = ViewFactory.CreateToggledLabel(
            nameof(Strings.FriendsEmptyTitle),
            nameof(FriendsPanelViewModel.IsEmpty),
            TextStyleCatalog.NoticeTitle);
        Label noMatchLabel = CreateNoMatchLabel();
        StackPanel loadFailure = CreateLoadFailure();
        AddChild(filterRow);
        AddChild(groups);
        AddChild(emptyLabel);
        AddChild(noMatchLabel);
        AddChild(loadFailure);

        _friendsPanelViewModel.ConnectedFriends.CollectionChanged += HandleConnectedFriendsChanged;
        _friendsPanelViewModel.OfflineFriends.CollectionChanged += HandleOfflineFriendsChanged;
    }

    private StackPanel CreateFilterRow()
    {
        var filter = new TextField
        {
            Placeholder = nameof(Strings.FriendsFilterPlaceholder),
            Width = FilterFieldWidth,
            Height = FilterHeight,
        };
        filter.SetBinding(nameof(TextField.Text), nameof(FriendsPanelViewModel.FilterText));

        Button addFriendButton = ViewFactory.CreateButton(
            nameof(Strings.FriendsAddFriend),
            AddFriendButtonWidth,
            FilterHeight);
        ButtonSkin.CreateAndAttach(addFriendButton, ButtonLookCatalog.Secondary);
        addFriendButton.Click += HandleAddFriendClick;

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = PanelMetrics.SectionSpacing,
        };
        row.AddChild(filter);
        row.AddChild(addFriendButton);
        return row;
    }

    private void HandleAddFriendClick(object? sender, EventArgs e)
    {
        _friendsPanelViewModel.OpenAddFriend();
    }

    private ScrollViewer CreateGroups()
    {
        ScrollViewer groups = ViewFactory.CreateScrollList<ScrollViewer>(ListWidth, ScrollHeight);
        Label connectedTitle = ViewFactory.CreateBoundNotice(
            nameof(FriendsPanelViewModel.ConnectedGroupTitle),
            nameof(FriendsPanelViewModel.HasConnectedFriends),
            TextStyleCatalog.SectionLabel);
        Label offlineTitle = ViewFactory.CreateBoundNotice(
            nameof(FriendsPanelViewModel.OfflineGroupTitle),
            nameof(FriendsPanelViewModel.HasOfflineFriends),
            TextStyleCatalog.SectionLabel);
        groups.InnerPanel.AddChild(connectedTitle.Visual);
        groups.InnerPanel.AddChild(_connectedRows.Visual);
        groups.InnerPanel.AddChild(offlineTitle.Visual);
        groups.InnerPanel.AddChild(_offlineRows.Visual);
        return groups;
    }

    private static Label CreateNoMatchLabel()
    {
        return ViewFactory.CreateBoundNotice(
            nameof(FriendsPanelViewModel.NoMatchText),
            nameof(FriendsPanelViewModel.HasNoMatch),
            TextStyleCatalog.NoticeTitle);
    }

    private StackPanel CreateLoadFailure()
    {
        StackPanel failure = ViewFactory.CreateLoadFailure(
            nameof(Strings.FriendsLoadFailed),
            nameof(Strings.FriendsRetry),
            HandleRetryClick);
        failure.SetBinding(nameof(IsVisible), nameof(FriendsPanelViewModel.HasLoadFailed));
        return failure;
    }

    private async void HandleRetryClick(object? sender, EventArgs e)
    {
        try
        {
            await _friendsPanelViewModel.LoadFriends();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The friend list could not be reloaded because the connection is closed.", ex);
        }
    }

    private void HandleConnectedFriendsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildRows(_connectedRows, _friendsPanelViewModel.ConnectedFriends);
    }

    private void RebuildRows(StackPanel rows, IEnumerable<FriendRowViewModel> friends)
    {
        ViewFactory.ClearRows(rows.Visual);
        foreach (FriendRowViewModel friend in friends)
        {
            var friendRow = new FriendRowView(friend, _friendsPanelViewModel.Actions);
            rows.AddChild(friendRow);
        }
    }

    private void HandleOfflineFriendsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildRows(_offlineRows, _friendsPanelViewModel.OfflineFriends);
    }
}
