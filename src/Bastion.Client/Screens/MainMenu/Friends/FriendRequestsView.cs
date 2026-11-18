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
        Spacing = PanelMetrics.SectionSpacing;

        StackPanel subtabRowStackPanel = CreateSubtabRow();
        Label statusLabel = ViewFactory.CreateBoundNotice(
            nameof(FriendRequestsViewModel.StatusText),
            nameof(FriendRequestsViewModel.HasStatus),
            TextStyleCatalog.FieldError);
        Label emptyTextLabel = ViewFactory.CreateBoundNotice(
            nameof(FriendRequestsViewModel.EmptyText),
            nameof(FriendRequestsViewModel.HasEmptyText),
            TextStyleCatalog.NoticeTitle);
        ItemsControl requestListItemsControl = CreateRequestList();
        AddChild(subtabRowStackPanel);
        AddChild(statusLabel);
        AddChild(emptyTextLabel);
        AddChild(requestListItemsControl);
    }

    private StackPanel CreateSubtabRow()
    {
        var rowStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = SubtabSpacing,
        };
        TabButton receivedTabButton = CreateSubtab(
            nameof(FriendRequestsViewModel.ReceivedTitle),
            nameof(FriendRequestsViewModel.IsReceivedTab),
            HandleReceivedClick);
        TabButton sentTabButton = CreateSubtab(
            nameof(FriendRequestsViewModel.SentTitle),
            nameof(FriendRequestsViewModel.IsSentTab),
            HandleSentClick);
        rowStackPanel.AddChild(receivedTabButton);
        rowStackPanel.AddChild(sentTabButton);
        return rowStackPanel;
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
        ItemsControl requestListItemsControl = ViewFactory.CreateScrollList<ItemsControl>(ViewWidth, RowsHeight);
        requestListItemsControl.VisualTemplate = new VisualTemplate(CreateRequestRow);
        requestListItemsControl.Items = _requests.VisibleRows;
        return requestListItemsControl;
    }

    private GraphicalUiElement CreateRequestRow(object? item)
    {
        FriendRequestRowViewModel row = ViewFactory.RequireRow<FriendRequestRowViewModel>(item);
        var friendRequestRowView = new FriendRequestRowView(_requests, row);
        return friendRequestRowView.Visual;
    }
}
