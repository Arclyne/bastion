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
        StackPanel text = CreateText(row);
        StackPanel side = CreateSide();
        ShapeFactory.InsertBehindContent(Visual, background);
        AddChild(text);
        AddChild(side);
    }

    private static StackPanel CreateText(ConversationRowViewModel row)
    {
        Label nickname = ViewFactory.CreateUntranslatedLabel(row.Partner.Nickname, TextStyleCatalog.IdentityName);

        Label preview = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(ConversationRowViewModel.PreviewText),
            TextStyleCatalog.Hint,
            TextWidth);
        var previewVisual = (LabelVisual)preview.Visual;
        previewVisual.MaxNumberOfLines = PreviewLines;

        StackPanel text = ViewFactory.CreateCenteredLines(nickname, preview);
        text.X = PaddingX;
        return text;
    }

    private StackPanel CreateSide()
    {
        Button openButton = ViewFactory.CreateButton(nameof(Strings.ChatOpen), OpenButtonWidth, OpenButtonHeight);
        ButtonSkin.CreateAndAttach(openButton, ButtonLookCatalog.Secondary);
        ViewFactory.CenterVertically(openButton);
        openButton.Click += HandleOpenClick;

        var side = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = SideSpacing,
        };
        Label time = CreateTime();
        Panel badge = CreateBadge();
        side.AddChild(time);
        side.AddChild(badge);
        side.AddChild(openButton);
        side.Anchor(Gum.Wireframe.Anchor.Right);
        side.X = -PaddingX;
        return side;
    }

    private void HandleOpenClick(object? sender, EventArgs e)
    {
        _chatTab.OpenPrivate(_row.Partner);
    }

    private static Label CreateTime()
    {
        Label time = ViewFactory.CreateBoundUntranslatedLabel(nameof(ConversationRowViewModel.TimeText), TextStyleCatalog.Hint);
        ViewFactory.CenterVertically(time);
        return time;
    }

    private static Panel CreateBadge()
    {
        var badge = new CountBadge(nameof(ConversationRowViewModel.UnreadText));
        ViewFactory.CenterVertically(badge);
        badge.SetBinding(nameof(IsVisible), nameof(ConversationRowViewModel.HasUnread));
        return badge;
    }
}
