using System;
using Bastion.Client.Controls;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed class ConversationRowView : Panel
{
    private const float RowWidth = PanelMetrics.ContentWidth;
    private const float RowHeight = 60;
    private const float RowRadius = 10;
    private const float PaddingX = 12;
    private const float TextSpacing = 2;
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
        Shapes.InsertBehindContent(Visual, Shapes.CreateRounded(Palette.Selected, RowRadius));
        AddChild(CreateText(row));
        AddChild(CreateSide());
    }

    private static StackPanel CreateText(ConversationRowViewModel row)
    {
        Label nickname = ViewFactory.CreateUntranslatedLabel(row.Partner.Nickname, TextStyles.IdentityName);

        Label preview = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(ConversationRowViewModel.PreviewText),
            TextStyles.Hint,
            TextWidth);
        var previewVisual = (LabelVisual)preview.Visual;
        previewVisual.MaxNumberOfLines = PreviewLines;

        var text = new StackPanel
        {
            Spacing = TextSpacing,
            X = PaddingX,
        };
        text.AddChild(nickname);
        text.AddChild(preview);
        ViewFactory.CenterVertically(text);
        return text;
    }

    private StackPanel CreateSide()
    {
        Button open = ViewFactory.CreateButton(nameof(Strings.ChatOpen), OpenButtonWidth, OpenButtonHeight);
        ButtonSkin.CreateAndAttach(open, ButtonLooks.Secondary);
        ViewFactory.CenterVertically(open);
        open.Click += HandleOpenClick;

        var side = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = SideSpacing,
        };
        side.AddChild(CreateTime());
        side.AddChild(CreateBadge());
        side.AddChild(open);
        side.Anchor(Gum.Wireframe.Anchor.Right);
        side.X = -PaddingX;
        return side;
    }

    private static Label CreateTime()
    {
        Label time = ViewFactory.CreateBoundUntranslatedLabel(nameof(ConversationRowViewModel.TimeText), TextStyles.Hint);
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

    private void HandleOpenClick(object? sender, EventArgs e)
    {
        _chatTab.OpenPrivate(_row.Partner);
    }
}
