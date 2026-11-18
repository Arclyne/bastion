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
    private readonly StackPanel _connectedRowsStackPanel;
    private readonly StackPanel _offlineRowsStackPanel;

    public FriendListView(FriendsPanelViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _friendsPanelViewModel = viewModel;
        _connectedRowsStackPanel = new StackPanel();
        _offlineRowsStackPanel = new StackPanel();
        Width = ListWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Spacing = PanelMetrics.SectionSpacing;

        StackPanel filterRowStackPanel = CreateFilterRow();
        ScrollViewer groupsScrollViewer = CreateGroups();
        Label emptyLabel = ViewFactory.CreateToggledLabel(
            nameof(Strings.FriendsEmptyTitle),
            nameof(FriendsPanelViewModel.IsEmpty),
            TextStyleCatalog.NoticeTitle);
        Label noMatchLabel = CreateNoMatchLabel();
        StackPanel loadFailureStackPanel = CreateLoadFailure();
        AddChild(filterRowStackPanel);
        AddChild(groupsScrollViewer);
        AddChild(emptyLabel);
        AddChild(noMatchLabel);
        AddChild(loadFailureStackPanel);

        _friendsPanelViewModel.ConnectedFriends.CollectionChanged += HandleConnectedFriendsChanged;
        _friendsPanelViewModel.OfflineFriends.CollectionChanged += HandleOfflineFriendsChanged;
    }

    private StackPanel CreateFilterRow()
    {
        var filterTextField = new TextField
        {
            Placeholder = nameof(Strings.FriendsFilterPlaceholder),
            Width = FilterFieldWidth,
            Height = FilterHeight,
        };
        filterTextField.SetBinding(nameof(TextField.Text), nameof(FriendsPanelViewModel.FilterText));

        Button addFriendButton = ViewFactory.CreateButton(
            nameof(Strings.FriendsAddFriend),
            AddFriendButtonWidth,
            FilterHeight);
        ButtonSkin.CreateAndAttach(addFriendButton, ButtonLookCatalog.Secondary);
        addFriendButton.Click += HandleAddFriendClick;

        var rowStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = PanelMetrics.SectionSpacing,
        };
        rowStackPanel.AddChild(filterTextField);
        rowStackPanel.AddChild(addFriendButton);
        return rowStackPanel;
    }

    private void HandleAddFriendClick(object? sender, EventArgs e)
    {
        _friendsPanelViewModel.OpenAddFriend();
    }

    private ScrollViewer CreateGroups()
    {
        ScrollViewer groupsScrollViewer = ViewFactory.CreateScrollList<ScrollViewer>(ListWidth, ScrollHeight);
        Label connectedTitleLabel = ViewFactory.CreateBoundNotice(
            nameof(FriendsPanelViewModel.ConnectedGroupTitle),
            nameof(FriendsPanelViewModel.HasConnectedFriends),
            TextStyleCatalog.SectionLabel);
        Label offlineTitleLabel = ViewFactory.CreateBoundNotice(
            nameof(FriendsPanelViewModel.OfflineGroupTitle),
            nameof(FriendsPanelViewModel.HasOfflineFriends),
            TextStyleCatalog.SectionLabel);
        groupsScrollViewer.InnerPanel.AddChild(connectedTitleLabel.Visual);
        groupsScrollViewer.InnerPanel.AddChild(_connectedRowsStackPanel.Visual);
        groupsScrollViewer.InnerPanel.AddChild(offlineTitleLabel.Visual);
        groupsScrollViewer.InnerPanel.AddChild(_offlineRowsStackPanel.Visual);
        return groupsScrollViewer;
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
        StackPanel failureStackPanel = ViewFactory.CreateLoadFailure(
            nameof(Strings.FriendsLoadFailed),
            nameof(Strings.FriendsRetry),
            HandleRetryClick);
        failureStackPanel.SetBinding(nameof(IsVisible), nameof(FriendsPanelViewModel.HasLoadFailed));
        return failureStackPanel;
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
        RebuildRows(_connectedRowsStackPanel, _friendsPanelViewModel.ConnectedFriends);
    }

    private void RebuildRows(StackPanel rowsStackPanel, IEnumerable<FriendRowViewModel> friends)
    {
        ViewFactory.ClearRows(rowsStackPanel.Visual);
        foreach (FriendRowViewModel friend in friends)
        {
            var friendRowView = new FriendRowView(friend, _friendsPanelViewModel.Actions);
            rowsStackPanel.AddChild(friendRowView);
        }
    }

    private void HandleOfflineFriendsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildRows(_offlineRowsStackPanel, _friendsPanelViewModel.OfflineFriends);
    }
}
