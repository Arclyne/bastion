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
    private const float ListSpacing = 10;
    private const float RetryButtonWidth = 140;
    private const float RetryButtonHeight = 40;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(FriendListView));

    private readonly FriendsPanelViewModel _viewModel;
    private readonly StackPanel _connectedRows = new StackPanel();
    private readonly StackPanel _offlineRows = new StackPanel();

    public FriendListView(FriendsPanelViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;
        Width = ListWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Spacing = ListSpacing;

        AddChild(CreateFilterRow());
        AddChild(CreateGroups());
        AddChild(CreateVisibleLabel(nameof(Strings.FriendsEmptyTitle), nameof(FriendsPanelViewModel.IsEmpty)));
        AddChild(CreateNoMatchLabel());
        AddChild(CreateLoadFailure());

        _viewModel.ConnectedFriends.CollectionChanged += HandleConnectedFriendsChanged;
        _viewModel.OfflineFriends.CollectionChanged += HandleOfflineFriendsChanged;
    }

    private StackPanel CreateFilterRow()
    {
        var filter = new TextField
        {
            Placeholder = nameof(Strings.FriendsFilterPlaceholder),
            Width = ListWidth - AddFriendButtonWidth - ListSpacing,
            Height = FilterHeight,
        };
        filter.SetBinding(nameof(TextField.Text), nameof(FriendsPanelViewModel.FilterText));

        Button addFriend = ViewFactory.CreateButton(
            nameof(Strings.FriendsAddFriend),
            AddFriendButtonWidth,
            FilterHeight);
        ButtonSkin.CreateAndAttach(addFriend, ButtonLooks.Secondary);
        addFriend.Click += HandleAddFriendClick;

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = ListSpacing,
        };
        row.AddChild(filter);
        row.AddChild(addFriend);
        return row;
    }

    private ScrollViewer CreateGroups()
    {
        ScrollViewer groups = ViewFactory.CreateScrollList<ScrollViewer>(ListWidth, ScrollHeight);
        groups.InnerPanel.AddChild(ViewFactory.CreateBoundNotice(
            nameof(FriendsPanelViewModel.ConnectedGroupTitle),
            nameof(FriendsPanelViewModel.HasConnectedFriends),
            TextStyles.SectionLabel).Visual);
        groups.InnerPanel.AddChild(_connectedRows.Visual);
        groups.InnerPanel.AddChild(ViewFactory.CreateBoundNotice(
            nameof(FriendsPanelViewModel.OfflineGroupTitle),
            nameof(FriendsPanelViewModel.HasOfflineFriends),
            TextStyles.SectionLabel).Visual);
        groups.InnerPanel.AddChild(_offlineRows.Visual);
        return groups;
    }

    private static Label CreateVisibleLabel(string textKey, string visibilityProperty)
    {
        Label label = ViewFactory.CreateLabel(textKey, TextStyles.NoticeTitle);
        label.SetBinding(nameof(IsVisible), visibilityProperty);
        return label;
    }

    private static Label CreateNoMatchLabel()
    {
        return ViewFactory.CreateBoundNotice(
            nameof(FriendsPanelViewModel.NoMatchText),
            nameof(FriendsPanelViewModel.HasNoMatch),
            TextStyles.NoticeTitle);
    }

    private StackPanel CreateLoadFailure()
    {
        Button retry = ViewFactory.CreateButton(nameof(Strings.FriendsRetry), RetryButtonWidth, RetryButtonHeight);
        ButtonSkin.CreateAndAttach(retry, ButtonLooks.Secondary);
        retry.Click += HandleRetryClick;

        var failure = new StackPanel
        {
            Spacing = ListSpacing,
        };
        failure.AddChild(ViewFactory.CreateLabel(nameof(Strings.FriendsLoadFailed), TextStyles.FieldError));
        failure.AddChild(retry);
        failure.SetBinding(nameof(IsVisible), nameof(FriendsPanelViewModel.HasLoadFailed));
        return failure;
    }

    private void RebuildRows(StackPanel rows, IEnumerable<FriendRowViewModel> friends)
    {
        ViewFactory.ClearRows(rows.Visual);
        foreach (FriendRowViewModel friend in friends)
        {
            var friendRow = new FriendRowView(friend, _viewModel.Actions);
            rows.AddChild(friendRow);
        }
    }

    private void HandleAddFriendClick(object? sender, EventArgs e)
    {
        _viewModel.OpenAddFriend();
    }

    private async void HandleRetryClick(object? sender, EventArgs e)
    {
        try
        {
            await _viewModel.LoadFriends();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The friend list could not be reloaded because the connection is closed.", ex);
        }
    }

    private void HandleConnectedFriendsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildRows(_connectedRows, _viewModel.ConnectedFriends);
    }

    private void HandleOfflineFriendsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildRows(_offlineRows, _viewModel.OfflineFriends);
    }
}
