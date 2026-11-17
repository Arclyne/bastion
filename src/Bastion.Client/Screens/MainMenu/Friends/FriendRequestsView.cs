using System;
using Bastion.Client.Controls;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms;
using Gum.Forms.Controls;
using Gum.Wireframe;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class FriendRequestsView : StackPanel
{
    private const float ViewWidth = PanelMetrics.ContentWidth;
    private const float ViewSpacing = 10;
    private const float SubtabWidth = (ViewWidth - SubtabSpacing) / SubtabCount;
    private const float SubtabHeight = 36;
    private const float SubtabSpacing = 6;
    private const int SubtabCount = 2;
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

        StackPanel subtabRow = CreateSubtabRow();
        Label status = ViewFactory.CreateBoundNotice(
            nameof(FriendRequestsViewModel.StatusText),
            nameof(FriendRequestsViewModel.HasStatus),
            TextStyleCatalog.FieldError);
        Label emptyText = ViewFactory.CreateBoundNotice(
            nameof(FriendRequestsViewModel.EmptyText),
            nameof(FriendRequestsViewModel.HasEmptyText),
            TextStyleCatalog.NoticeTitle);
        ItemsControl requestList = CreateRequestList();
        AddChild(subtabRow);
        AddChild(status);
        AddChild(emptyText);
        AddChild(requestList);
    }

    private StackPanel CreateSubtabRow()
    {
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = SubtabSpacing,
        };
        TabButton receivedTab = CreateSubtab(
            nameof(FriendRequestsViewModel.ReceivedTitle),
            nameof(FriendRequestsViewModel.IsReceivedTab),
            HandleReceivedClick);
        TabButton sentTab = CreateSubtab(
            nameof(FriendRequestsViewModel.SentTitle),
            nameof(FriendRequestsViewModel.IsSentTab),
            HandleSentClick);
        row.AddChild(receivedTab);
        row.AddChild(sentTab);
        return row;
    }

    private static TabButton CreateSubtab(string titleProperty, string selectedProperty, EventHandler handleClick)
    {
        Button button = ViewFactory.CreateBoundUntranslatedButton(titleProperty, SubtabWidth, SubtabHeight);
        ButtonSkin.CreateAndAttach(button, ButtonLookCatalog.Pill);
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
}
