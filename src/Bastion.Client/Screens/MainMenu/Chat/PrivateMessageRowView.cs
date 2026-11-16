using System;
using Bastion.Client.Theme;
using Gum.Converters;
using Gum.DataTypes;
using Gum.Forms;
using Gum.Forms.Controls;
using Gum.GueDeriving;
using Gum.Wireframe;
using Microsoft.Xna.Framework;
using RenderingLibrary.Graphics;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed class PrivateMessageRowView : StackPanel
{
    private const float RowSpacing = 3;
    private const float BubbleTextMaxWidth = 246;
    private const float BubblePaddingX = 12;
    private const float BubblePaddingY = 8;
    private const float BubbleRadius = 14;
    private const float MetaSpacing = 6;
    private const int HorizontalSideCount = 2;
    private const int VerticalSideCount = 2;

    public PrivateMessageRowView(PrivateMessageRowViewModel row, float rowWidth)
    {
        ArgumentNullException.ThrowIfNull(row);

        BindingContext = row;
        Width = rowWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Spacing = RowSpacing;
        if (row.HasDayLabel())
        {
            Panel day = CreateDayLabel(row.DayLabel, rowWidth);
            AddChild(day);
        }

        Panel bubble = CreateBubble(row);
        Panel alignedBubble = CreateAligned(bubble, row.IsOwn, rowWidth);
        StackPanel meta = CreateMeta();
        Panel alignedMeta = CreateAligned(meta, row.IsOwn, rowWidth);
        AddChild(alignedBubble);
        AddChild(alignedMeta);
    }

    public static VisualTemplate CreateTemplate(float rowWidth)
    {
        return new VisualTemplate(item => CreateVisual(item, rowWidth));
    }

    private static Panel CreateDayLabel(string dayLabel, float rowWidth)
    {
        Label day = ViewFactory.CreateUntranslatedLabel(dayLabel, TextStyleCatalog.SectionLabel);
        ViewFactory.CenterHorizontally(day);
        return CreateAligned(day, shouldAlignRight: false, rowWidth);
    }

    private static Panel CreateAligned(FrameworkElement content, bool shouldAlignRight, float rowWidth)
    {
        if (shouldAlignRight)
        {
            content.XUnits = GeneralUnitType.PixelsFromLarge;
            content.XOrigin = HorizontalAlignment.Right;
            content.X = 0;
        }

        var holder = new Panel
        {
            Width = rowWidth,
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
        RectangleRuntime background = ShapeFactory.CreateRounded(fill, BubbleRadius);
        ShapeFactory.InsertBehindContent(bubble.Visual, background);
        bubble.AddChild(text);
        return bubble;
    }

    private static StackPanel CreateMeta()
    {
        Label time = ViewFactory.CreateBoundUntranslatedLabel(nameof(PrivateMessageRowViewModel.TimeText), TextStyleCatalog.Hint);
        Label pending = ViewFactory.CreateBoundNotice(
            nameof(PrivateMessageRowViewModel.StateText),
            nameof(PrivateMessageRowViewModel.IsPending),
            TextStyleCatalog.Hint);
        Label failed = ViewFactory.CreateBoundNotice(
            nameof(PrivateMessageRowViewModel.StateText),
            nameof(PrivateMessageRowViewModel.HasFailed),
            TextStyleCatalog.FieldError);

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

    private static GraphicalUiElement CreateVisual(object? item, float rowWidth)
    {
        PrivateMessageRowViewModel row = ViewFactory.RequireRow<PrivateMessageRowViewModel>(item);
        var rowView = new PrivateMessageRowView(row, rowWidth);
        return rowView.Visual;
    }
}
