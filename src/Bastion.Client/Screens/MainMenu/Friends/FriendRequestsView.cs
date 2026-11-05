using System;
using System.Collections.Specialized;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
using Gum.GueDeriving;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class FriendRequestsView : StackPanel
{
    private const float ViewWidth = 360;
    private const float ViewSpacing = 10;
    private const float SubtabWidth = 177;
    private const float SubtabHeight = 36;
    private const float SubtabSpacing = 6;
    private const float MarkerHeight = 3;
    private const float MarkerRadius = 1.5f;
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

        _rows = new ScrollViewer
        {
            Width = ViewWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = RowsHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        Shapes.HideDefaultParts(_rows);

        AddChild(CreateSubtabRow());
        AddChild(CreateBoundLabel(
            nameof(FriendRequestsViewModel.StatusText),
            nameof(FriendRequestsViewModel.HasStatus),
            TextStyles.FieldError));
        AddChild(CreateBoundLabel(
            nameof(FriendRequestsViewModel.EmptyText),
            nameof(FriendRequestsViewModel.HasEmptyText),
            TextStyles.NoticeTitle));
        AddChild(_rows);

        requests.VisibleRows.CollectionChanged += HandleVisibleRowsChanged;
    }

    private static Label CreateBoundLabel(string textProperty, string visibilityProperty, TextStyle style)
    {
        Label label = ViewFactory.CreateLabel(string.Empty, style);
        ((LabelVisual)label.Visual).LocalizeText = false;
        label.SetBinding(nameof(Label.Text), textProperty);
        label.SetBinding(nameof(IsVisible), visibilityProperty);
        return label;
    }

    private static StackPanel CreateSubtab(string titleProperty, string selectedProperty, EventHandler handleClick)
    {
        Button button = ViewFactory.CreateButton(string.Empty, SubtabWidth, SubtabHeight);
        ButtonSkin.CreateAndAttach(button, ButtonLooks.Pill);
        ((ButtonVisual)button.Visual).TextInstance.LocalizeText = false;
        button.SetBinding(nameof(Button.Text), titleProperty);
        button.Click += handleClick;

        RectangleRuntime marker = Shapes.CreateRounded(Palette.Accent, MarkerRadius);
        var markerHolder = new Panel
        {
            Width = SubtabWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = MarkerHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        markerHolder.AddChild(marker);
        markerHolder.SetBinding(nameof(IsVisible), selectedProperty);

        var subtab = new StackPanel { Spacing = SubtabSpacing };
        subtab.AddChild(button);
        subtab.AddChild(markerHolder);
        return subtab;
    }

    private StackPanel CreateSubtabRow()
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = SubtabSpacing };
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

    private void HandleVisibleRowsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        while (_rows.InnerPanel.Children.Count > 0)
        {
            _rows.InnerPanel.Children.RemoveAt(_rows.InnerPanel.Children.Count - 1);
        }

        foreach (FriendRequestRowViewModel row in _requests.VisibleRows)
        {
            _rows.InnerPanel.AddChild(new FriendRequestRowView(_requests, row).Visual);
        }
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
