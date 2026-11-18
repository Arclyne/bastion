using System;
using Bastion.Client.Controls;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
using Gum.GueDeriving;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed class ConversationRowView : Panel
{
    private const float RowWidth = PanelMetrics.ContentWidth;
    private const float RowHeight = 60;
    private const float PaddingX = 12;
    private const float TextWidth = 190;
    private const float SideSpacing = 8;
    private const float OpenButtonWidth = 76;
    private const float OpenButtonHeight = 34;
    private const int PreviewLines = 1;

    private readonly ConversationRowViewModel _row;
    private readonly ChatTabViewModel _chatTab;

    public ConversationRowView(ConversationRowViewModel row, ChatTabViewModel chatTab)
    {
        ArgumentNullException.ThrowIfNull(row);
        ArgumentNullException.ThrowIfNull(chatTab);

        _row = row;
        _chatTab = chatTab;
        BindingContext = row;
        Width = RowWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Height = RowHeight;
        HeightUnits = DimensionUnitType.Absolute;
        RectangleRuntime background = ShapeFactory.CreateRounded(Palette.Selected, PanelMetrics.RowRadius);
        StackPanel textStackPanel = CreateText(row);
        StackPanel sideStackPanel = CreateSide();
        ShapeFactory.InsertBehindContent(Visual, background);
        AddChild(textStackPanel);
        AddChild(sideStackPanel);
    }

    private static StackPanel CreateText(ConversationRowViewModel row)
    {
        Label nicknameLabel = ViewFactory.CreateUntranslatedLabel(row.Partner.Nickname, TextStyleCatalog.IdentityName);

        Label previewLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(ConversationRowViewModel.PreviewText),
            TextStyleCatalog.Hint,
            TextWidth);
        var previewVisual = (LabelVisual)previewLabel.Visual;
        previewVisual.MaxNumberOfLines = PreviewLines;

        StackPanel textStackPanel = ViewFactory.CreateCenteredLines(nicknameLabel, previewLabel);
        textStackPanel.X = PaddingX;
        return textStackPanel;
    }

    private StackPanel CreateSide()
    {
        Button openButton = ViewFactory.CreateButton(nameof(Strings.ChatOpen), OpenButtonWidth, OpenButtonHeight);
        ButtonSkin.CreateAndAttach(openButton, ButtonLookCatalog.Secondary);
        ViewFactory.CenterVertically(openButton);
        openButton.Click += HandleOpenClick;

        var sideStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = SideSpacing,
        };
        Label timeLabel = CreateTime();
        Panel badgePanel = CreateBadge();
        sideStackPanel.AddChild(timeLabel);
        sideStackPanel.AddChild(badgePanel);
        sideStackPanel.AddChild(openButton);
        sideStackPanel.Anchor(Gum.Wireframe.Anchor.Right);
        sideStackPanel.X = -PaddingX;
        return sideStackPanel;
    }

    private void HandleOpenClick(object? sender, EventArgs e)
    {
        _chatTab.OpenPrivate(_row.Partner);
    }

    private static Label CreateTime()
    {
        Label timeLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(ConversationRowViewModel.TimeText),
            TextStyleCatalog.Hint);
        ViewFactory.CenterVertically(timeLabel);
        return timeLabel;
    }

    private static Panel CreateBadge()
    {
        var countBadge = new CountBadge(nameof(ConversationRowViewModel.UnreadText));
        ViewFactory.CenterVertically(countBadge);
        countBadge.SetBinding(nameof(IsVisible), nameof(ConversationRowViewModel.HasUnread));
        return countBadge;
    }
}
