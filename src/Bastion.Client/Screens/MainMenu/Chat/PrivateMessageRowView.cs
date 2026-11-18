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
            Panel dayPanel = CreateDayLabel(row.DayLabel, rowWidth);
            AddChild(dayPanel);
        }

        Panel bubblePanel = CreateBubble(row);
        Panel alignedBubblePanel = CreateAligned(bubblePanel, row.IsOwn, rowWidth);
        StackPanel metaStackPanel = CreateMeta();
        Panel alignedMetaPanel = CreateAligned(metaStackPanel, row.IsOwn, rowWidth);
        AddChild(alignedBubblePanel);
        AddChild(alignedMetaPanel);
    }

    public static VisualTemplate CreateTemplate(float rowWidth)
    {
        return new VisualTemplate(item => CreateVisual(item, rowWidth));
    }

    private static Panel CreateDayLabel(string dayText, float rowWidth)
    {
        Label dayLabel = ViewFactory.CreateUntranslatedLabel(dayText, TextStyleCatalog.SectionLabel);
        ViewFactory.CenterHorizontally(dayLabel);
        return CreateAligned(dayLabel, shouldAlignRight: false, rowWidth);
    }

    private static Panel CreateAligned(FrameworkElement content, bool shouldAlignRight, float rowWidth)
    {
        if (shouldAlignRight)
        {
            content.XUnits = GeneralUnitType.PixelsFromLarge;
            content.XOrigin = HorizontalAlignment.Right;
            content.X = 0;
        }

        var holderPanel = new Panel
        {
            Width = rowWidth,
            WidthUnits = DimensionUnitType.Absolute,
            HeightUnits = DimensionUnitType.RelativeToChildren,
        };
        holderPanel.AddChild(content);
        return holderPanel;
    }

    private static Panel CreateBubble(PrivateMessageRowViewModel row)
    {
        Label textLabel = ViewFactory.CreateUntranslatedLabel(row.Text, TextStyleCatalog.NoticeMessage);
        textLabel.WidthUnits = DimensionUnitType.RelativeToChildren;
        textLabel.Visual.MaxWidth = BubbleTextMaxWidth;
        textLabel.X = BubblePaddingX;
        textLabel.Y = BubblePaddingY;

        var bubblePanel = new Panel
        {
            Width = BubblePaddingX * HorizontalSideCount,
            WidthUnits = DimensionUnitType.RelativeToChildren,
            Height = BubblePaddingY * VerticalSideCount,
            HeightUnits = DimensionUnitType.RelativeToChildren,
        };
        Color fillColor = row.IsOwn ? Palette.WarningSoft : Palette.White;
        RectangleRuntime background = ShapeFactory.CreateRounded(fillColor, BubbleRadius);
        ShapeFactory.InsertBehindContent(bubblePanel.Visual, background);
        bubblePanel.AddChild(textLabel);
        return bubblePanel;
    }

    private static StackPanel CreateMeta()
    {
        Label timeLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(PrivateMessageRowViewModel.TimeText),
            TextStyleCatalog.Hint);
        Label pendingLabel = ViewFactory.CreateBoundNotice(
            nameof(PrivateMessageRowViewModel.StateText),
            nameof(PrivateMessageRowViewModel.IsPending),
            TextStyleCatalog.Hint);
        Label failedLabel = ViewFactory.CreateBoundNotice(
            nameof(PrivateMessageRowViewModel.StateText),
            nameof(PrivateMessageRowViewModel.HasFailed),
            TextStyleCatalog.FieldError);

        var metaStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = MetaSpacing,
        };
        metaStackPanel.AddChild(timeLabel);
        metaStackPanel.AddChild(pendingLabel);
        metaStackPanel.AddChild(failedLabel);
        return metaStackPanel;
    }

    private static GraphicalUiElement CreateVisual(object? item, float rowWidth)
    {
        PrivateMessageRowViewModel row = ViewFactory.RequireRow<PrivateMessageRowViewModel>(item);
        var privateMessageRowView = new PrivateMessageRowView(row, rowWidth);
        return privateMessageRowView.Visual;
    }
}
