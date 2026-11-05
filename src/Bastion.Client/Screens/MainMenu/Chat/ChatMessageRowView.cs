using System;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed class ChatMessageRowView : StackPanel
{
    private const float RowWidth = 340;
    private const float RowSpacing = 2;
    private const float HeaderSpacing = 8;

    public ChatMessageRowView(ChatMessageRowViewModel row)
    {
        ArgumentNullException.ThrowIfNull(row);

        BindingContext = row;
        Width = RowWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Spacing = RowSpacing;
        AddChild(CreateHeader(row));
        AddChild(CreateFixedLabel(row.Text, TextStyles.NoticeMessage));
        AddChild(CreateStateLabel(nameof(ChatMessageRowViewModel.IsPending), TextStyles.Hint));
        AddChild(CreateStateLabel(nameof(ChatMessageRowViewModel.HasFailed), TextStyles.FieldError));
    }

    private static StackPanel CreateHeader(ChatMessageRowViewModel row)
    {
        TextStyle authorStyle = row.IsOwn ? TextStyles.ChatOwnAuthor : TextStyles.ChatAuthor;
        Label time = CreateUnlocalizedLabel(TextStyles.Hint);
        time.SetBinding(nameof(Label.Text), nameof(ChatMessageRowViewModel.TimeText));

        Label author = CreateUnlocalizedLabel(authorStyle);
        author.Text = row.AuthorText;

        var header = new StackPanel { Orientation = Orientation.Horizontal, Spacing = HeaderSpacing };
        header.AddChild(author);
        header.AddChild(time);
        return header;
    }

    private static Label CreateFixedLabel(string text, TextStyle style)
    {
        Label label = CreateUnlocalizedLabel(style);
        label.Text = text;
        label.Width = RowWidth;
        label.WidthUnits = DimensionUnitType.Absolute;
        return label;
    }

    private static Label CreateStateLabel(string visibilityProperty, TextStyle style)
    {
        Label label = CreateUnlocalizedLabel(style);
        label.Width = RowWidth;
        label.WidthUnits = DimensionUnitType.Absolute;
        label.SetBinding(nameof(Label.Text), nameof(ChatMessageRowViewModel.StateText));
        label.SetBinding(nameof(IsVisible), visibilityProperty);
        return label;
    }

    private static Label CreateUnlocalizedLabel(TextStyle style)
    {
        Label label = ViewFactory.CreateLabel(string.Empty, style);
        ((LabelVisual)label.Visual).LocalizeText = false;
        return label;
    }
}
