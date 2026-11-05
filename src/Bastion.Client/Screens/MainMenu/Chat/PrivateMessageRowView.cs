using System;
using Bastion.Client.Theme;
using Gum.Converters;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
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

    public PrivateMessageRowView(PrivateMessageRowViewModel row)
    {
        ArgumentNullException.ThrowIfNull(row);

        BindingContext = row;
        Width = RowWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Spacing = RowSpacing;
        if (row.HasDayLabel)
        {
            AddChild(CreateDayLabel(row.DayLabel));
        }

        AddChild(CreateAligned(CreateBubble(row), row.IsOwn));
        AddChild(CreateAligned(CreateMeta(), row.IsOwn));
    }

    private static Label CreateUnlocalizedLabel(TextStyle style)
    {
        Label label = ViewFactory.CreateLabel(string.Empty, style);
        ((LabelVisual)label.Visual).LocalizeText = false;
        return label;
    }

    private static Panel CreateDayLabel(string dayLabel)
    {
        Label day = CreateUnlocalizedLabel(TextStyles.SectionLabel);
        day.Text = dayLabel;
        day.XUnits = GeneralUnitType.PixelsFromMiddle;
        day.XOrigin = HorizontalAlignment.Center;

        var holder = new Panel
        {
            Width = RowWidth,
            WidthUnits = DimensionUnitType.Absolute,
            HeightUnits = DimensionUnitType.RelativeToChildren,
        };
        holder.AddChild(day);
        return holder;
    }

    private static Panel CreateBubble(PrivateMessageRowViewModel row)
    {
        Label text = CreateUnlocalizedLabel(TextStyles.NoticeMessage);
        text.Text = row.Text;
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
        Shapes.InsertBehindContent(bubble.Visual, Shapes.CreateRounded(fill, BubbleRadius));
        bubble.AddChild(text);
        return bubble;
    }

    private static StackPanel CreateMeta()
    {
        Label time = CreateUnlocalizedLabel(TextStyles.Hint);
        time.SetBinding(nameof(Label.Text), nameof(PrivateMessageRowViewModel.TimeText));
        Label pending = CreateUnlocalizedLabel(TextStyles.Hint);
        pending.SetBinding(nameof(Label.Text), nameof(PrivateMessageRowViewModel.StateText));
        pending.SetBinding(nameof(IsVisible), nameof(PrivateMessageRowViewModel.IsPending));
        Label failed = CreateUnlocalizedLabel(TextStyles.FieldError);
        failed.SetBinding(nameof(Label.Text), nameof(PrivateMessageRowViewModel.StateText));
        failed.SetBinding(nameof(IsVisible), nameof(PrivateMessageRowViewModel.HasFailed));

        var meta = new StackPanel { Orientation = Orientation.Horizontal, Spacing = MetaSpacing };
        meta.AddChild(time);
        meta.AddChild(pending);
        meta.AddChild(failed);
        return meta;
    }

    private static Panel CreateAligned(FrameworkElement content, bool alignRight)
    {
        if (alignRight)
        {
            content.XUnits = GeneralUnitType.PixelsFromLarge;
            content.XOrigin = HorizontalAlignment.Right;
            content.X = 0;
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
}
