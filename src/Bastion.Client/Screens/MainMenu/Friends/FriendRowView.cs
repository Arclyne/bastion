using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.GueDeriving;
using Microsoft.Xna.Framework;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class FriendRowView : Panel
{
    private const float RowWidth = PanelMetrics.ContentWidth;
    private const float RowHeight = 52;
    private const float DotSize = 10;
    private const float ContentPaddingX = 6;
    private const float ContentSpacing = 12;
    private const float TextSpacing = 2;
    private const float MoreButtonWidth = 64;
    private const float MoreButtonHeight = 32;

    private readonly FriendRowViewModel _viewModel;
    private readonly FriendActionsViewModel _actions;

    public FriendRowView(FriendRowViewModel viewModel, FriendActionsViewModel actions)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(actions);

        _viewModel = viewModel;
        _actions = actions;
        BindingContext = viewModel;
        Width = RowWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Height = RowHeight;
        HeightUnits = DimensionUnitType.Absolute;

        var content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = ContentSpacing,
        };
        content.AddChild(CreatePresenceDot(viewModel.IsConnected));
        content.AddChild(CreateText());
        content.Anchor(Gum.Wireframe.Anchor.Left);
        content.X = ContentPaddingX;
        AddChild(content);
        AddChild(CreateMoreButton());
    }

    private static Panel CreatePresenceDot(bool isConnected)
    {
        var dot = new Panel
        {
            Width = DotSize,
            WidthUnits = DimensionUnitType.Absolute,
            Height = DotSize,
            HeightUnits = DimensionUnitType.Absolute,
        };
        Color color = isConnected ? Palette.Success : Palette.Muted;
        RectangleRuntime circle = ShapeFactory.CreateCircle(color, DotSize);
        ShapeFactory.InsertBehindContent(dot.Visual, circle);
        ViewFactory.CenterVertically(dot);
        return dot;
    }

    private static StackPanel CreateText()
    {
        var text = new StackPanel
        {
            Spacing = TextSpacing,
        };
        text.AddChild(CreateBoundLabel(nameof(FriendRowViewModel.Nickname), TextStyleCatalog.IdentityName));
        text.AddChild(CreateBoundLabel(nameof(FriendRowViewModel.StatusText), TextStyleCatalog.Hint));
        ViewFactory.CenterVertically(text);
        return text;
    }

    private static Label CreateBoundLabel(string propertyName, TextStyle style)
    {
        return ViewFactory.CreateBoundUntranslatedLabel(propertyName, style);
    }

    private Button CreateMoreButton()
    {
        Button more = ViewFactory.CreateButton(nameof(Strings.FriendsMoreOptions), MoreButtonWidth, MoreButtonHeight);
        ButtonSkin.CreateAndAttach(more, ButtonLookCatalog.Pill);
        more.Anchor(Gum.Wireframe.Anchor.Right);
        more.Click += HandleMoreClick;
        return more;
    }

    private void HandleMoreClick(object? sender, EventArgs e)
    {
        _actions.OpenMenu(_viewModel);
    }
}
