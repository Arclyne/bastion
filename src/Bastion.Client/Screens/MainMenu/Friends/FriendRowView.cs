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
    private const float ContentSpacing = 12;
    private const float MoreButtonWidth = 64;
    private const float MoreButtonHeight = 32;

    private readonly FriendRowViewModel _row;
    private readonly FriendActionsViewModel _actions;

    public FriendRowView(FriendRowViewModel viewModel, FriendActionsViewModel actions)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(actions);

        _row = viewModel;
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
        Panel presenceDot = CreatePresenceDot(viewModel.IsConnected);
        StackPanel text = CreateText();
        content.AddChild(presenceDot);
        content.AddChild(text);
        content.Anchor(Gum.Wireframe.Anchor.Left);
        content.X = PanelMetrics.RowContentPaddingX;
        Button moreButton = CreateMoreButton();
        AddChild(content);
        AddChild(moreButton);
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
            Spacing = PanelMetrics.TextSpacing,
        };
        Label nickname = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(FriendRowViewModel.Nickname),
            TextStyleCatalog.IdentityName);
        Label status = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(FriendRowViewModel.StatusText),
            TextStyleCatalog.Hint);
        text.AddChild(nickname);
        text.AddChild(status);
        ViewFactory.CenterVertically(text);
        return text;
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
        _actions.OpenMenu(_row);
    }
}
