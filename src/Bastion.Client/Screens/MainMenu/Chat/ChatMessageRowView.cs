using System;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms;
using Gum.Forms.Controls;
using Gum.Wireframe;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed class ChatMessageRowView : StackPanel
{
    private const float HeaderSpacing = 8;

    public ChatMessageRowView(ChatMessageRowViewModel row, float rowWidth)
    {
        ArgumentNullException.ThrowIfNull(row);

        BindingContext = row;
        Width = rowWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Spacing = PanelMetrics.TextSpacing;
        StackPanel header = CreateHeader(row);
        Label text = CreateFixedLabel(row.Text, TextStyleCatalog.NoticeMessage, rowWidth);
        Label pendingState = CreateStateLabel(
            nameof(ChatMessageRowViewModel.IsPending),
            TextStyleCatalog.Hint,
            rowWidth);
        Label failedState = CreateStateLabel(
            nameof(ChatMessageRowViewModel.HasFailed),
            TextStyleCatalog.FieldError,
            rowWidth);
        AddChild(header);
        AddChild(text);
        AddChild(pendingState);
        AddChild(failedState);
    }

    public static VisualTemplate CreateTemplate(float rowWidth)
    {
        return new VisualTemplate(item => CreateVisual(item, rowWidth));
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

    private static Label CreateFixedLabel(string text, TextStyle style, float width)
    {
        Label label = ViewFactory.CreateUntranslatedLabel(text, style);
        ViewFactory.SetFixedWidth(label, width);
        return label;
    }

    private static Label CreateStateLabel(string visibilityProperty, TextStyle style, float width)
    {
        Label label = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(ChatMessageRowViewModel.StateText),
            style,
            width);
        label.SetBinding(nameof(IsVisible), visibilityProperty);
        return label;
    }

    private static GraphicalUiElement CreateVisual(object? item, float rowWidth)
    {
        ChatMessageRowViewModel row = ViewFactory.RequireRow<ChatMessageRowViewModel>(item);
        var rowView = new ChatMessageRowView(row, rowWidth);
        return rowView.Visual;
    }
}
