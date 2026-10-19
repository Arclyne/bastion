using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.Converters;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
using Gum.GueDeriving;
using Gum.Wireframe;
using RenderingLibrary.Graphics;

namespace Bastion.Client.Screens.MainMenu;

public sealed class MainMenuView : Panel
{
    private const float ScreenMarginX = 24;
    private const float TopMarginY = 22;
    private const float BottomMarginY = 22;
    private const float PillHeight = 44;
    private const float SettingsWidth = 116;
    private const float HowToPlayWidth = 140;
    private const float SettingsOffsetY = 9;
    private const float ChipHeight = 62;
    private const float ChipRadius = 31;
    private const float ChipPaddingLeft = 8;
    private const float ChipPaddingRight = 20;
    private const float ChipSpacing = 12;
    private const float ChipBorderWidth = 1.5f;
    private const float ChipShadowOffsetY = 8;
    private const float ChipShadowBlur = 22;
    private const float AvatarSize = 46;
    private const float AvatarRingWidth = 2.5f;
    private const float IdentitySpacing = 3;
    private const float TabletWidth = 460;
    private const float TabletY = 164;
    private const float TabletPaddingX = 30;
    private const float TabletPaddingTop = 26;
    private const float TabletPaddingBottom = 30;
    private const float TabletContentWidth = 400;
    private const float TabletSpacing = 20;
    private const float BrandSpacing = 3;
    private const float HeroHeight = 78;
    private const float HeroIconSize = 50;
    private const float HeroIconRadius = 14;
    private const float HeroPaddingX = 14;
    private const float HeroTextX = 78;
    private const float HeroTextSpacing = 3;
    private const float PrivateButtonWidth = 193;
    private const float PrivateButtonHeight = 52;
    private const float PrivateButtonSpacing = 14;

    public MainMenuView(MainMenuViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        BindingContext = viewModel;
        Dock(Gum.Wireframe.Dock.Fill);
        AddChild(CreateIdentityChip());
        AddChild(CreateSettingsButton());
        AddChild(CreateTablet());
        AddChild(CreateHowToPlayButton());
        AddChild(ViewFactory.CreateVersionLabel());
    }

    private static Panel CreateIdentityChip()
    {
        var content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = ChipSpacing };
        content.AddChild(CreateAvatar());
        content.AddChild(CreateIdentityText());
        content.Anchor(Gum.Wireframe.Anchor.Left);
        content.X = ChipPaddingLeft;

        var chip = new Panel
        {
            X = ScreenMarginX,
            Y = TopMarginY,
            Width = ChipPaddingRight,
            WidthUnits = DimensionUnitType.RelativeToChildren,
            Height = ChipHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        chip.AddChild(content);

        RectangleRuntime surface = Shapes.CreateRounded(Palette.Card, ChipRadius);
        surface.StrokeColor = Palette.Border;
        surface.StrokeWidth = ChipBorderWidth;
        surface.HasDropshadow = true;
        surface.DropshadowColor = Palette.Shadow;
        surface.DropshadowOffsetY = ChipShadowOffsetY;
        surface.DropshadowBlur = ChipShadowBlur;
        Shapes.InsertBehindContent(chip.Visual, surface);
        return chip;
    }

    private static Panel CreateAvatar()
    {
        var avatar = new Panel
        {
            Width = AvatarSize,
            WidthUnits = DimensionUnitType.Absolute,
            Height = AvatarSize,
            HeightUnits = DimensionUnitType.Absolute,
        };
        RectangleRuntime circle = Shapes.CreateRounded(Palette.AvatarFill, AvatarSize / 2);
        circle.StrokeColor = Palette.PlayerOne;
        circle.StrokeWidth = AvatarRingWidth;
        Shapes.InsertBehindContent(avatar.Visual, circle);
        return avatar;
    }

    private static StackPanel CreateIdentityText()
    {
        Label nickname = ViewFactory.CreateLabel(string.Empty, TextStyles.IdentityName);
        ((LabelVisual)nickname.Visual).LocalizeText = false;
        nickname.SetBinding(nameof(Label.Text), nameof(MainMenuViewModel.Nickname));

        var text = new StackPanel { Spacing = IdentitySpacing };
        text.AddChild(nickname);
        text.AddChild(ViewFactory.CreateLabel(nameof(Strings.MainMenuClassicMode), TextStyles.IdentityDetail));
        CenterVertically(text);
        return text;
    }

    private static Button CreateSettingsButton()
    {
        Button button = CreatePillButton(nameof(Strings.MainMenuSettings), SettingsWidth);
        button.Anchor(Gum.Wireframe.Anchor.TopRight);
        button.X = -ScreenMarginX;
        button.Y = TopMarginY + SettingsOffsetY;
        return button;
    }

    private static Button CreateHowToPlayButton()
    {
        Button button = CreatePillButton(nameof(Strings.MainMenuHowToPlay), HowToPlayWidth);
        button.Anchor(Gum.Wireframe.Anchor.BottomLeft);
        button.X = ScreenMarginX;
        button.Y = -BottomMarginY;
        return button;
    }

    private static Button CreatePillButton(string textKey, float width)
    {
        Button button = ViewFactory.CreateButton(textKey, width, PillHeight);
        ButtonSkin.Apply(button, ButtonLooks.Pill);
        return button;
    }

    private static Panel CreateTablet()
    {
        var content = new StackPanel
        {
            X = TabletPaddingX,
            Y = TabletPaddingTop,
            Width = TabletContentWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Spacing = TabletSpacing,
        };
        content.AddChild(CreateBrand());
        content.AddChild(CreateFindMatchButton());
        content.AddChild(CreateCentered(ViewFactory.CreateLabel(
            nameof(Strings.MainMenuPrivateMatch),
            TextStyles.SectionLabel)));
        content.AddChild(CreatePrivateMatchButtons());

        var tablet = new Panel
        {
            Width = TabletWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = TabletPaddingBottom,
            HeightUnits = DimensionUnitType.RelativeToChildren,
        };
        tablet.Anchor(Gum.Wireframe.Anchor.Top);
        tablet.Y = TabletY;
        tablet.AddChild(content);
        Skins.ApplyCard(tablet.Visual);
        return tablet;
    }

    private static Panel CreateBrand()
    {
        var brand = new StackPanel { Spacing = BrandSpacing };
        brand.AddChild(ViewFactory.CreateLabel(nameof(Strings.AppName), TextStyles.MenuTitle));
        brand.AddChild(ViewFactory.CreateLabel(nameof(Strings.LoginTagline), TextStyles.MenuTagline));
        return CreateCentered(brand);
    }

    private static Button CreateFindMatchButton()
    {
        Button button = ViewFactory.CreateButton(string.Empty, TabletContentWidth, HeroHeight);
        ButtonSkin.Apply(button, ButtonLooks.Hero);

        RectangleRuntime icon = Shapes.CreateRounded(Palette.HeroIconFill, HeroIconRadius);
        icon.Anchor(Gum.Wireframe.Anchor.Left);
        icon.X = HeroPaddingX;
        icon.Width = HeroIconSize;
        icon.WidthUnits = DimensionUnitType.Absolute;
        icon.Height = HeroIconSize;
        icon.HeightUnits = DimensionUnitType.Absolute;
        button.AddChild(icon);

        var text = new StackPanel { X = HeroTextX, Spacing = HeroTextSpacing };
        text.AddChild(ViewFactory.CreateLabel(nameof(Strings.MainMenuFindMatch), TextStyles.HeroTitle));
        text.AddChild(ViewFactory.CreateLabel(nameof(Strings.MainMenuFindMatchDetail), TextStyles.HeroDetail));
        CenterVertically(text);
        button.AddChild(text);
        return button;
    }

    private static StackPanel CreatePrivateMatchButtons()
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = PrivateButtonSpacing };
        row.AddChild(CreateSecondaryButton(nameof(Strings.MainMenuCreateRoom)));
        row.AddChild(CreateSecondaryButton(nameof(Strings.MainMenuJoinWithCode)));
        return row;
    }

    private static Button CreateSecondaryButton(string textKey)
    {
        Button button = ViewFactory.CreateButton(textKey, PrivateButtonWidth, PrivateButtonHeight);
        ButtonSkin.Apply(button, ButtonLooks.Secondary);
        return button;
    }

    private static Panel CreateCentered(FrameworkElement element)
    {
        element.XUnits = GeneralUnitType.PixelsFromMiddle;
        element.XOrigin = HorizontalAlignment.Center;
        element.X = 0;

        var row = new Panel
        {
            Width = TabletContentWidth,
            WidthUnits = DimensionUnitType.Absolute,
            HeightUnits = DimensionUnitType.RelativeToChildren,
        };
        row.AddChild(element);
        return row;
    }

    private static void CenterVertically(FrameworkElement element)
    {
        element.YUnits = GeneralUnitType.PixelsFromMiddle;
        element.YOrigin = VerticalAlignment.Center;
        element.Y = 0;
    }
}
