using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Bastion.Client.Controls;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class FriendListView : StackPanel
{
    private const float ListWidth = 360;
    private const float FilterHeight = 44;
    private const float ScrollHeight = 460;
    private const float ListSpacing = 10;
    private const float RetryButtonWidth = 140;
    private const float RetryButtonHeight = 40;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(FriendListView));

    private readonly FriendsPanelViewModel _viewModel;
    private readonly StackPanel _menuRows = new StackPanel();
    private readonly StackPanel _offlineRows = new StackPanel();

    public FriendListView(FriendsPanelViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;
        Width = ListWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Spacing = ListSpacing;

        AddChild(CreateFilter());
        AddChild(CreateGroups());
        AddChild(CreateVisibleLabel(nameof(Strings.FriendsEmptyTitle), nameof(FriendsPanelViewModel.IsEmpty)));
        AddChild(CreateNoMatchLabel());
        AddChild(CreateLoadFailure());

        _viewModel.MenuFriends.CollectionChanged += HandleMenuFriendsChanged;
        _viewModel.OfflineFriends.CollectionChanged += HandleOfflineFriendsChanged;
    }

    private static TextField CreateFilter()
    {
        var filter = new TextField
        {
            Placeholder = nameof(Strings.FriendsFilterPlaceholder),
            Width = ListWidth,
            Height = FilterHeight,
        };
        filter.SetBinding(nameof(TextField.Text), nameof(FriendsPanelViewModel.FilterText));
        return filter;
    }

    private static Label CreateVisibleLabel(string textKey, string visibilityProperty)
    {
        Label label = ViewFactory.CreateLabel(textKey, TextStyles.NoticeTitle);
        label.SetBinding(nameof(IsVisible), visibilityProperty);
        return label;
    }

    private static Label CreateBoundLabel(string textProperty, string visibilityProperty, TextStyle style)
    {
        Label label = ViewFactory.CreateLabel(string.Empty, style);
        ((LabelVisual)label.Visual).LocalizeText = false;
        label.SetBinding(nameof(Label.Text), textProperty);
        label.SetBinding(nameof(IsVisible), visibilityProperty);
        return label;
    }

    private static Label CreateNoMatchLabel()
    {
        return CreateBoundLabel(
            nameof(FriendsPanelViewModel.NoMatchText),
            nameof(FriendsPanelViewModel.HasNoMatch),
            TextStyles.NoticeTitle);
    }

    private static void RebuildRows(StackPanel rows, IEnumerable<FriendRowViewModel> friends)
    {
        while (rows.Visual.Children.Count > 0)
        {
            rows.Visual.Children.RemoveAt(rows.Visual.Children.Count - 1);
        }

        foreach (FriendRowViewModel friend in friends)
        {
            rows.AddChild(new FriendRowView(friend));
        }
    }

    private ScrollViewer CreateGroups()
    {
        var groups = new ScrollViewer
        {
            Width = ListWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = ScrollHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        Shapes.HideDefaultParts(groups);
        groups.InnerPanel.AddChild(CreateBoundLabel(
            nameof(FriendsPanelViewModel.MenuGroupTitle),
            nameof(FriendsPanelViewModel.HasMenuFriends),
            TextStyles.SectionLabel).Visual);
        groups.InnerPanel.AddChild(_menuRows.Visual);
        groups.InnerPanel.AddChild(CreateBoundLabel(
            nameof(FriendsPanelViewModel.OfflineGroupTitle),
            nameof(FriendsPanelViewModel.HasOfflineFriends),
            TextStyles.SectionLabel).Visual);
        groups.InnerPanel.AddChild(_offlineRows.Visual);
        return groups;
    }

    private StackPanel CreateLoadFailure()
    {
        Button retry = ViewFactory.CreateButton(nameof(Strings.FriendsRetry), RetryButtonWidth, RetryButtonHeight);
        ButtonSkin.CreateAndAttach(retry, ButtonLooks.Secondary);
        retry.Click += HandleRetryClick;

        var failure = new StackPanel { Spacing = ListSpacing };
        failure.AddChild(ViewFactory.CreateLabel(nameof(Strings.FriendsLoadFailed), TextStyles.FieldError));
        failure.AddChild(retry);
        failure.SetBinding(nameof(IsVisible), nameof(FriendsPanelViewModel.HasLoadFailed));
        return failure;
    }

    private void HandleMenuFriendsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildRows(_menuRows, _viewModel.MenuFriends);
    }

    private void HandleOfflineFriendsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildRows(_offlineRows, _viewModel.OfflineFriends);
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
}
