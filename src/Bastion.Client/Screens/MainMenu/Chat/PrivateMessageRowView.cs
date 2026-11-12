using System;
using Bastion.Client.Theme;
using Gum.Converters;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Wireframe;
using Microsoft.Xna.Framework;
using RenderingLibrary.Graphics;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed class PrivateMessageRowView : StackPanel
{
    private const float RowWidth = 340;
    private const float RowSpacing = 3;
    private const float BubbleTextMaxWidth = 246;
    private const float BubblePaddingX = 12;
    private const float BubblePaddingY = 8;
    private const float BubbleRadius = 14;
    private const float MetaSpacing = 6;
    private const int HorizontalSideCount = 2;
    private const int VerticalSideCount = 2;
    private const float RightEdgeOffsetX = 0;

    public PrivateMessageRowView(PrivateMessageRowViewModel row)
    {
        ArgumentNullException.ThrowIfNull(row);

        BindingContext = row;
        Width = RowWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Spacing = RowSpacing;
        if (row.HasDayLabel())
        {
            AddChild(CreateDayLabel(row.DayLabel));
        }

        AddChild(CreateAligned(CreateBubble(row), row.IsOwn));
        AddChild(CreateAligned(CreateMeta(), row.IsOwn));
    }

    public static GraphicalUiElement CreateVisual(object? item)
    {
        PrivateMessageRowViewModel row = ViewFactory.RequireRow<PrivateMessageRowViewModel>(item);
        var rowView = new PrivateMessageRowView(row);
        return rowView.Visual;
    }

    private static Panel CreateDayLabel(string dayLabel)
    {
        Label day = ViewFactory.CreateUntranslatedLabel(dayLabel, TextStyleCatalog.SectionLabel);
        ViewFactory.CenterHorizontally(day);

        var holder = new Panel
        {
            Width = RowWidth,
            WidthUnits = DimensionUnitType.Absolute,
            HeightUnits = DimensionUnitType.RelativeToChildren,
        };
        holder.AddChild(day);
        return holder;
    }

    private static Panel CreateAligned(FrameworkElement content, bool alignRight)
    {
        if (alignRight)
        {
            content.XUnits = GeneralUnitType.PixelsFromLarge;
            content.XOrigin = HorizontalAlignment.Right;
            content.X = RightEdgeOffsetX;
        }

        var holder = new Panel
        {
            Width = RowWidth,
            WidthUnits = DimensionUnitType.Absolute,
            HeightUnits = DimensionUnitType.RelativeToChildren,
        };
        holder.AddChild(content);
        return holder;
    }

    private static Panel CreateBubble(PrivateMessageRowViewModel row)
    {
        Label text = ViewFactory.CreateUntranslatedLabel(row.Text, TextStyleCatalog.NoticeMessage);
        text.WidthUnits = DimensionUnitType.RelativeToChildren;
        text.Visual.MaxWidth = BubbleTextMaxWidth;
        text.X = BubblePaddingX;
        text.Y = BubblePaddingY;

        var bubble = new Panel
        {
            Width = BubblePaddingX * HorizontalSideCount,
            WidthUnits = DimensionUnitType.RelativeToChildren,
            Height = BubblePaddingY * VerticalSideCount,
            HeightUnits = DimensionUnitType.RelativeToChildren,
        };
        Color fill = row.IsOwn ? Palette.WarningSoft : Palette.White;
        ShapeFactory.InsertBehindContent(bubble.Visual, ShapeFactory.CreateRounded(fill, BubbleRadius));
        bubble.AddChild(text);
        return bubble;
    }

    private static StackPanel CreateMeta()
    {
        Label time = ViewFactory.CreateBoundUntranslatedLabel(nameof(PrivateMessageRowViewModel.TimeText), TextStyleCatalog.Hint);
        Label pending = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(PrivateMessageRowViewModel.StateText),
            TextStyleCatalog.Hint);
        pending.SetBinding(nameof(IsVisible), nameof(PrivateMessageRowViewModel.IsPending));
        Label failed = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(PrivateMessageRowViewModel.StateText),
            TextStyleCatalog.FieldError);
        failed.SetBinding(nameof(IsVisible), nameof(PrivateMessageRowViewModel.HasFailed));

        var meta = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = MetaSpacing,
        };
        meta.AddChild(time);
        meta.AddChild(pending);
        meta.AddChild(failed);
        return meta;
    }
}
