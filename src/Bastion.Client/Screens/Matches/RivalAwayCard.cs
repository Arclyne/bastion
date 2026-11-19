using System;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.GueDeriving;

namespace Bastion.Client.Screens.Matches;

public sealed class RivalAwayCard : Panel
{
    private const float CardWidth = 440;
    private const float PaddingX = 18;
    private const float PaddingTop = 16;
    private const float PaddingBottom = 16;
    private const float Spacing = 12;
    private const int HorizontalSideCount = 2;
    private const float ContentWidth = CardWidth - (PaddingX * HorizontalSideCount);
    private const float CountdownWidth = 70;
    private const float HeadingSpacing = 10;
    private const float TextWidth = ContentWidth - CountdownWidth - HeadingSpacing;
    private const float TextSpacing = 2;
    private const float ProgressHeight = 8;
    private const float ProgressRadius = 4;

    public RivalAwayCard(MatchConnectionViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        BindingContext = viewModel;
        Width = CardWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Height = PaddingBottom;
        HeightUnits = DimensionUnitType.RelativeToChildren;
        SetBinding(nameof(IsVisible), nameof(MatchConnectionViewModel.HasAwayRival));
        var contentStackPanel = new StackPanel
        {
            X = PaddingX,
            Y = PaddingTop,
            Spacing = Spacing,
        };
        Panel headingPanel = CreateHeading();
        Panel progressPanel = CreateProgress();
        contentStackPanel.AddChild(headingPanel);
        contentStackPanel.AddChild(progressPanel);
        AddChild(contentStackPanel);
        CardSkin.Apply(Visual);
    }

    private static Panel CreateHeading()
    {
        var headingPanel = new Panel
        {
            Width = ContentWidth,
            WidthUnits = DimensionUnitType.Absolute,
            HeightUnits = DimensionUnitType.RelativeToChildren,
        };
        var textStackPanel = new StackPanel
        {
            Spacing = TextSpacing,
        };
        Label titleLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(MatchConnectionViewModel.RivalTitleText),
            TextStyleCatalog.RivalAwayTitle,
            TextWidth);
        Label detailLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(MatchConnectionViewModel.RivalDetailText),
            TextStyleCatalog.RivalAwayDetail,
            TextWidth);
        textStackPanel.AddChild(titleLabel);
        textStackPanel.AddChild(detailLabel);
        headingPanel.AddChild(textStackPanel);
        headingPanel.AddChild(CreateCountdown(
            nameof(MatchConnectionViewModel.IsCountdownRunning),
            TextStyleCatalog.CountdownRunning));
        headingPanel.AddChild(CreateCountdown(
            nameof(MatchConnectionViewModel.IsCountdownEnding),
            TextStyleCatalog.CountdownEnding));
        return headingPanel;
    }

    private static Label CreateCountdown(string visibilityProperty, TextStyle style)
    {
        Label countdownLabel = ViewFactory.CreateBoundNotice(
            nameof(MatchConnectionViewModel.CountdownText),
            visibilityProperty,
            style);
        countdownLabel.Anchor(Gum.Wireframe.Anchor.Right);
        return countdownLabel;
    }

    private static Panel CreateProgress()
    {
        var trackPanel = new Panel
        {
            Width = ContentWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = ProgressHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        RectangleRuntime track = ShapeFactory.CreateRounded(Palette.ProgressTrack, ProgressRadius);
        ShapeFactory.InsertBehindContent(trackPanel.Visual, track);
        var fillPanel = new Panel
        {
            WidthUnits = DimensionUnitType.PercentageOfParent,
            Height = ProgressHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        fillPanel.SetBinding(nameof(Width), nameof(MatchConnectionViewModel.RivalProgressPercent));
        RectangleRuntime fill = ShapeFactory.CreateRounded(Palette.Warning, ProgressRadius);
        ShapeFactory.InsertBehindContent(fillPanel.Visual, fill);
        trackPanel.AddChild(fillPanel);
        return trackPanel;
    }
}
