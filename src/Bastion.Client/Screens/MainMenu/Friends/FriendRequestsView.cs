using System;
using System.Collections.Specialized;
using Bastion.Client.Controls;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class FriendRequestsView : StackPanel
{
    private const float ViewWidth = PanelMetrics.ContentWidth;
    private const float ViewSpacing = 10;
    private const float SubtabWidth = 177;
    private const float SubtabHeight = 36;
    private const float SubtabSpacing = 6;
    private const float RowsHeight = 470;

    private readonly FriendRequestsViewModel _requests;
    private readonly ScrollViewer _rows;

    public FriendRequestsView(FriendRequestsViewModel requests)
    {
        ArgumentNullException.ThrowIfNull(requests);

        _requests = requests;
        BindingContext = requests;
        Width = ViewWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Spacing = ViewSpacing;

        _rows = ViewFactory.CreateScrollList(ViewWidth, RowsHeight);

        AddChild(CreateSubtabRow());
        AddChild(ViewFactory.CreateBoundNotice(
            nameof(FriendRequestsViewModel.StatusText),
            nameof(FriendRequestsViewModel.HasStatus),
            TextStyles.FieldError));
        AddChild(ViewFactory.CreateBoundNotice(
            nameof(FriendRequestsViewModel.EmptyText),
            nameof(FriendRequestsViewModel.HasEmptyText),
            TextStyles.NoticeTitle));
        AddChild(_rows);

        requests.VisibleRows.CollectionChanged += HandleVisibleRowsChanged;
    }

    private StackPanel CreateSubtabRow()
    {
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = SubtabSpacing,
        };
        row.AddChild(CreateSubtab(
            nameof(FriendRequestsViewModel.ReceivedTitle),
            nameof(FriendRequestsViewModel.ShowsReceived),
            HandleReceivedClick));
        row.AddChild(CreateSubtab(
            nameof(FriendRequestsViewModel.SentTitle),
            nameof(FriendRequestsViewModel.ShowsSent),
            HandleSentClick));
        return row;
    }

    private static TabButton CreateSubtab(string titleProperty, string selectedProperty, EventHandler handleClick)
    {
        Button button = ViewFactory.CreateButton(string.Empty, SubtabWidth, SubtabHeight);
        ButtonSkin.CreateAndAttach(button, ButtonLooks.Pill);
        var buttonVisual = (ButtonVisual)button.Visual;
        buttonVisual.TextInstance.LocalizeText = false;
        button.SetBinding(nameof(Button.Text), titleProperty);
        button.Click += handleClick;
        return new TabButton(button, selectedProperty, SubtabSpacing);
    }

    private void HandleReceivedClick(object? sender, EventArgs e)
    {
        _requests.ShowReceived();
    }

    private void HandleSentClick(object? sender, EventArgs e)
    {
        _requests.ShowSent();
    }

    private void HandleVisibleRowsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ViewFactory.ClearRows(_rows.InnerPanel);
        foreach (FriendRequestRowViewModel row in _requests.VisibleRows)
        {
            var rowView = new FriendRequestRowView(_requests, row);
            _rows.InnerPanel.AddChild(rowView.Visual);
        }
    }
}
