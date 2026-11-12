using System;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Wireframe;

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
        AddChild(CreateFixedLabel(row.Text, TextStyleCatalog.NoticeMessage));
        AddChild(CreateStateLabel(nameof(ChatMessageRowViewModel.IsPending), TextStyleCatalog.Hint));
        AddChild(CreateStateLabel(nameof(ChatMessageRowViewModel.HasFailed), TextStyleCatalog.FieldError));
    }

    public static GraphicalUiElement CreateVisual(object? item)
    {
        ChatMessageRowViewModel row = ViewFactory.RequireRow<ChatMessageRowViewModel>(item);
        var rowView = new ChatMessageRowView(row);
        return rowView.Visual;
    }

    private static StackPanel CreateHeader(ChatMessageRowViewModel row)
    {
        TextStyle authorStyle = row.IsOwn ? TextStyleCatalog.ChatOwnAuthor : TextStyleCatalog.ChatAuthor;
        Label time = ViewFactory.CreateBoundUntranslatedLabel(nameof(ChatMessageRowViewModel.TimeText), TextStyleCatalog.Hint);
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
