using System;
using Bastion.Client.Controls;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
using Gum.Wireframe;

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

    public FriendRequestsView(FriendRequestsViewModel requests)
    {
        ArgumentNullException.ThrowIfNull(requests);

        _requests = requests;
        BindingContext = requests;
        Width = ViewWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Spacing = ViewSpacing;

        AddChild(CreateSubtabRow());
        AddChild(ViewFactory.CreateBoundNotice(
            nameof(FriendRequestsViewModel.StatusText),
            nameof(FriendRequestsViewModel.HasStatus),
            TextStyleCatalog.FieldError));
        AddChild(ViewFactory.CreateBoundNotice(
            nameof(FriendRequestsViewModel.EmptyText),
            nameof(FriendRequestsViewModel.HasEmptyText),
            TextStyleCatalog.NoticeTitle));
        AddChild(CreateRequestList());
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
        ButtonSkin.CreateAndAttach(button, ButtonLookCatalog.Pill);
        var buttonVisual = (ButtonVisual)button.Visual;
        buttonVisual.TextInstance.LocalizeText = false;
        button.SetBinding(nameof(Button.Text), titleProperty);
        button.Click += handleClick;
        return new TabButton(button, selectedProperty, SubtabSpacing);
    }

    private ItemsControl CreateRequestList()
    {
        ItemsControl requestList = ViewFactory.CreateScrollList<ItemsControl>(ViewWidth, RowsHeight);
        requestList.VisualTemplate = new VisualTemplate(CreateRequestRow);
        requestList.Items = _requests.VisibleRows;
        return requestList;
    }

    private GraphicalUiElement CreateRequestRow(object? item)
    {
        FriendRequestRowViewModel row = ViewFactory.RequireRow<FriendRequestRowViewModel>(item);
        var rowView = new FriendRequestRowView(_requests, row);
        return rowView.Visual;
    }

    private void HandleReceivedClick(object? sender, EventArgs e)
    {
        _requests.ShowReceived();
    }

    private void HandleSentClick(object? sender, EventArgs e)
    {
        _requests.ShowSent();
    }
}
