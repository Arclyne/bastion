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
    private const float DotSize = 10;
    private const float ContentSpacing = 12;
    private const float MoreButtonWidth = 64;
    private const float MoreButtonHeight = 32;

    private readonly FriendRowViewModel _row;
    private readonly FriendActionsViewModel _actions;

    public FriendRowView(FriendRowViewModel row, FriendActionsViewModel actions)
    {
        ArgumentNullException.ThrowIfNull(row);
        ArgumentNullException.ThrowIfNull(actions);

        _row = row;
        _actions = actions;
        BindingContext = row;
        Width = RowWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Height = PanelMetrics.PlayerRowHeight;
        HeightUnits = DimensionUnitType.Absolute;

        var contentStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = ContentSpacing,
        };
        Panel presenceDotPanel = CreatePresenceDot(row.IsConnected);
        StackPanel textStackPanel = CreateText();
        contentStackPanel.AddChild(presenceDotPanel);
        contentStackPanel.AddChild(textStackPanel);
        contentStackPanel.Anchor(Gum.Wireframe.Anchor.Left);
        contentStackPanel.X = PanelMetrics.RowContentPaddingX;
        Button moreButton = CreateMoreButton();
        AddChild(contentStackPanel);
        AddChild(moreButton);
    }

    private static Panel CreatePresenceDot(bool isConnected)
    {
        var dotPanel = new Panel
        {
            Width = DotSize,
            WidthUnits = DimensionUnitType.Absolute,
            Height = DotSize,
            HeightUnits = DimensionUnitType.Absolute,
        };
        Color color = isConnected ? Palette.Success : Palette.Muted;
        RectangleRuntime circle = ShapeFactory.CreateCircle(color, DotSize);
        ShapeFactory.InsertBehindContent(dotPanel.Visual, circle);
        ViewFactory.CenterVertically(dotPanel);
        return dotPanel;
    }

    private static StackPanel CreateText()
    {
        Label nicknameLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(FriendRowViewModel.Nickname),
            TextStyleCatalog.IdentityName);
        Label statusLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(FriendRowViewModel.StatusText),
            TextStyleCatalog.Hint);
        return ViewFactory.CreateCenteredLines(nicknameLabel, statusLabel);
    }

    private Button CreateMoreButton()
    {
        Button moreButton = ViewFactory.CreateButton(
            nameof(Strings.FriendsMoreOptions),
            MoreButtonWidth,
            MoreButtonHeight);
        ButtonSkin.CreateAndAttach(moreButton, ButtonLookCatalog.Pill);
        moreButton.Anchor(Gum.Wireframe.Anchor.Right);
        moreButton.Click += HandleMoreClick;
        return moreButton;
    }

    private void HandleMoreClick(object? sender, EventArgs e)
    {
        _actions.OpenMenu(_row);
    }
}
