using System;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;

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
        Label time = ViewFactory.CreateBoundUntranslatedLabel(nameof(ChatMessageRowViewModel.TimeText), TextStyles.Hint);
        Label author = ViewFactory.CreateUntranslatedLabel(row.AuthorText, authorStyle);

        var header = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = HeaderSpacing,
        };
        header.AddChild(author);
        header.AddChild(time);
        return header;
    }

    private static Label CreateFixedLabel(string text, TextStyle style)
    {
        Label label = ViewFactory.CreateUntranslatedLabel(text, style);
        label.Width = RowWidth;
        label.WidthUnits = DimensionUnitType.Absolute;
        return label;
    }

    private static Label CreateStateLabel(string visibilityProperty, TextStyle style)
    {
        Label label = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(ChatMessageRowViewModel.StateText),
            style,
            RowWidth);
        label.SetBinding(nameof(IsVisible), visibilityProperty);
        return label;
    }
}
