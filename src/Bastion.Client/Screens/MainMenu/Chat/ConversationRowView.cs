using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.Converters;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
using RenderingLibrary.Graphics;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed class ConversationRowView : Panel
{
    private const float RowWidth = 360;
    private const float RowHeight = 60;
    private const float RowRadius = 10;
    private const float PaddingX = 12;
    private const float TextSpacing = 2;
    private const float TextWidth = 190;
    private const float SideSpacing = 8;
    private const float BadgeSize = 22;
    private const float OpenButtonWidth = 76;
    private const float OpenButtonHeight = 34;
    private const int PreviewLines = 1;

    private readonly ConversationRowViewModel _row;
    private readonly ChatTabViewModel _tab;

    public ConversationRowView(ConversationRowViewModel row, ChatTabViewModel tab)
    {
        ArgumentNullException.ThrowIfNull(row);
        ArgumentNullException.ThrowIfNull(tab);

        _row = row;
        _tab = tab;
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
        Label nickname = ViewFactory.CreateLabel(string.Empty, TextStyles.IdentityName);
        ((LabelVisual)nickname.Visual).LocalizeText = false;
        nickname.Text = row.Nickname;

        Label preview = ViewFactory.CreateLabel(string.Empty, TextStyles.Hint);
        var previewVisual = (LabelVisual)preview.Visual;
        previewVisual.LocalizeText = false;
        previewVisual.MaxNumberOfLines = PreviewLines;
        preview.Width = TextWidth;
        preview.WidthUnits = DimensionUnitType.Absolute;
        preview.SetBinding(nameof(Label.Text), nameof(ConversationRowViewModel.PreviewText));

        var text = new StackPanel { Spacing = TextSpacing, X = PaddingX };
        text.AddChild(nickname);
        text.AddChild(preview);
        ViewFactory.CenterVertically(text);
        return text;
    }

    private static Label CreateTime()
    {
        Label time = ViewFactory.CreateLabel(string.Empty, TextStyles.Hint);
        ((LabelVisual)time.Visual).LocalizeText = false;
        time.SetBinding(nameof(Label.Text), nameof(ConversationRowViewModel.TimeText));
        ViewFactory.CenterVertically(time);
        return time;
    }

    private static Panel CreateBadge()
    {
        Label count = ViewFactory.CreateLabel(string.Empty, TextStyles.PrimaryButton);
        ((LabelVisual)count.Visual).LocalizeText = false;
        count.SetBinding(nameof(Label.Text), nameof(ConversationRowViewModel.UnreadText));
        count.XUnits = GeneralUnitType.PixelsFromMiddle;
        count.XOrigin = HorizontalAlignment.Center;
        ViewFactory.CenterVertically(count);

        var badge = new Panel
        {
            Width = BadgeSize,
            WidthUnits = DimensionUnitType.Absolute,
            Height = BadgeSize,
            HeightUnits = DimensionUnitType.Absolute,
        };
        Shapes.InsertBehindContent(badge.Visual, Shapes.CreateRounded(Palette.PlayerOne, BadgeSize / 2));
        badge.AddChild(count);
        ViewFactory.CenterVertically(badge);
        badge.SetBinding(nameof(IsVisible), nameof(ConversationRowViewModel.HasUnread));
        return badge;
    }

    private StackPanel CreateSide()
    {
        Button open = ViewFactory.CreateButton(nameof(Strings.ChatOpen), OpenButtonWidth, OpenButtonHeight);
        ButtonSkin.CreateAndAttach(open, ButtonLooks.Secondary);
        ViewFactory.CenterVertically(open);
        open.Click += HandleOpenClick;

        var side = new StackPanel { Orientation = Orientation.Horizontal, Spacing = SideSpacing };
        side.AddChild(CreateTime());
        side.AddChild(CreateBadge());
        side.AddChild(open);
        side.Anchor(Gum.Wireframe.Anchor.Right);
        side.X = -PaddingX;
        return side;
    }

    private void HandleOpenClick(object? sender, EventArgs e)
    {
        _tab.OpenPrivate(_row.Partner);
    }
}
