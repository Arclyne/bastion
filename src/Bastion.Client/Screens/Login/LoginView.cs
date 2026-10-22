using System;
using Bastion.Client.Controls;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;

namespace Bastion.Client.Screens.Login;

public sealed class LoginView : Panel
{
    private const float CardX = 96;
    private const float CardWidth = 440;
    private const float CardPaddingX = 32;
    private const float CardPaddingTop = 28;
    private const float CardPaddingBottom = 26;
    private const float ContentWidth = 376;
    private const float SectionSpacing = 18;
    private const float FieldSpacing = 7;
    private const float HeaderSpacing = 4;
    private const float LanguagePickerWidth = 140;
    private const float LanguagePickerHeight = 36;
    private const float LanguagePickerMarginY = 8;
    private const float FieldHeight = 50;
    private const float RevealButtonWidth = 96;
    private const float RevealButtonSpacing = 8;
    private const float PrimaryButtonHeight = 52;
    private const float SecondaryButtonHeight = 48;
    private const float ExitButtonWidth = 96;
    private const float ExitButtonHeight = 42;
    private const float ScreenMarginX = 24;
    private const float ExitButtonMarginY = 22;

    private readonly LoginViewModel _viewModel;
    private readonly ModalLayer _dialogLayer;

    public LoginView(LoginViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;
        BindingContext = viewModel;
        Dock(Gum.Wireframe.Dock.Fill);
        AddChild(CreateCard());
        AddChild(CreateExitButton());
        AddChild(ViewFactory.CreateVersionLabel());
        _dialogLayer = CreateDialogLayer(viewModel);
    }

    private Panel CreateCard()
    {
        var card = new Panel();
        card.Anchor(Gum.Wireframe.Anchor.Left);
        card.X = CardX;
        card.Width = CardWidth;
        card.WidthUnits = DimensionUnitType.Absolute;
        card.Height = CardPaddingBottom;
        card.HeightUnits = DimensionUnitType.RelativeToChildren;
        card.AddChild(CreateCardContent());
        Skins.ApplyCard(card.Visual);
        return card;
    }

    private StackPanel CreateCardContent()
    {
        var content = new StackPanel
        {
            X = CardPaddingX,
            Y = CardPaddingTop,
            Width = ContentWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Spacing = SectionSpacing,
        };
        content.AddChild(CreateHeader());
        content.AddChild(CreateIdentifierField());
        content.AddChild(CreatePasswordField());
        content.AddChild(CreateNotice());
        content.AddChild(CreateLogInButton());
        content.AddChild(ViewFactory.CreateLabel(nameof(Strings.LoginNoAccount), TextStyles.Divider));
        content.AddChild(CreateCreateAccountButton());
        content.AddChild(CreateTwoFactorHint());
        return content;
    }

    private static Panel CreateHeader()
    {
        var brand = new StackPanel { Spacing = HeaderSpacing };
        brand.AddChild(ViewFactory.CreateLabel(nameof(Strings.AppName), TextStyles.Title));
        brand.AddChild(ViewFactory.CreateLabel(nameof(Strings.LoginTagline), TextStyles.Tagline));

        var header = new Panel
        {
            Width = ContentWidth,
            WidthUnits = DimensionUnitType.Absolute,
            HeightUnits = DimensionUnitType.RelativeToChildren,
        };
        header.AddChild(brand);
        header.AddChild(CreateLanguagePicker());
        return header;
    }

    private static LanguagePicker CreateLanguagePicker()
    {
        var picker = new LanguagePicker(LanguagePickerWidth, LanguagePickerHeight);
        picker.SetBinding(nameof(LanguagePicker.Items), nameof(LoginViewModel.Languages));
        picker.SetBinding(nameof(LanguagePicker.SelectedObject), nameof(LoginViewModel.SelectedLanguage));
        BindEnabledToIdle(picker);
        picker.Anchor(Gum.Wireframe.Anchor.TopRight);
        picker.Y = LanguagePickerMarginY;
        return picker;
    }

    private static StackPanel CreateIdentifierField()
    {
        var input = new TextField
        {
            Placeholder = nameof(Strings.LoginIdentifierPlaceholder),
            Width = ContentWidth,
            Height = FieldHeight,
        };
        input.SetBinding(nameof(TextField.Text), nameof(LoginViewModel.Identifier));
        input.SetBinding(nameof(TextField.HasError), nameof(LoginViewModel.IsIdentifierMissing));
        BindEnabledToIdle(input);

        Label warning = CreateWarning(
            nameof(Strings.LoginIdentifierRequired),
            nameof(LoginViewModel.IsIdentifierMissing));
        return CreateFieldGroup(nameof(Strings.LoginIdentifierLabel), input, warning);
    }

    private static StackPanel CreatePasswordField()
    {
        Label warning = CreateWarning(nameof(Strings.LoginPasswordRequired), nameof(LoginViewModel.IsPasswordMissing));
        return CreateFieldGroup(nameof(Strings.LoginPasswordLabel), CreatePasswordInputs(), warning);
    }

    private static StackPanel CreatePasswordInputs()
    {
        float inputWidth = ContentWidth - RevealButtonWidth - RevealButtonSpacing;

        var hiddenInput = new PasswordField { Placeholder = string.Empty, Width = inputWidth, Height = FieldHeight };
        hiddenInput.SetBinding(nameof(PasswordField.Password), nameof(LoginViewModel.Password));
        hiddenInput.SetBinding(nameof(PasswordField.HasError), nameof(LoginViewModel.IsPasswordMissing));
        hiddenInput.SetBinding(nameof(IsVisible), nameof(LoginViewModel.IsPasswordHidden));
        BindEnabledToIdle(hiddenInput);

        var visibleInput = new TextField { Placeholder = string.Empty, Width = inputWidth, Height = FieldHeight };
        visibleInput.SetBinding(nameof(TextField.Text), nameof(LoginViewModel.Password));
        visibleInput.SetBinding(nameof(TextField.HasError), nameof(LoginViewModel.IsPasswordMissing));
        visibleInput.SetBinding(nameof(IsVisible), nameof(LoginViewModel.IsPasswordVisible));
        BindEnabledToIdle(visibleInput);

        var revealButton = new ToggleButton
        {
            Text = nameof(Strings.LoginShowPasswordButton),
            Width = RevealButtonWidth,
            Height = FieldHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        revealButton.SetBinding(nameof(ToggleButton.IsChecked), nameof(LoginViewModel.IsPasswordVisible));
        ButtonSkin.Apply(revealButton, ButtonLooks.Secondary);
        BindEnabledToIdle(revealButton);

        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = RevealButtonSpacing };
        row.AddChild(hiddenInput);
        row.AddChild(visibleInput);
        row.AddChild(revealButton);
        return row;
    }

    private static StackPanel CreateFieldGroup(string captionKey, FrameworkElement input, Label warning)
    {
        var group = new StackPanel { Spacing = FieldSpacing };
        group.AddChild(ViewFactory.CreateLabel(captionKey, TextStyles.FieldLabel));
        group.AddChild(input);
        group.AddChild(warning);
        return group;
    }

    private static Label CreateWarning(string textKey, string visibilityPath)
    {
        Label warning = ViewFactory.CreateLabel(textKey, TextStyles.FieldError);
        warning.SetBinding(nameof(IsVisible), visibilityPath);
        return warning;
    }

    private Button CreateLogInButton()
    {
        Button button = ViewFactory.CreateButton(nameof(Strings.LoginButton), ContentWidth, PrimaryButtonHeight);
        ButtonSkin.Apply(button, ButtonLooks.Primary);
        button.SetBinding(nameof(Button.Text), nameof(LoginViewModel.LogInButtonTextKey));
        button.Click += OnLogInClick;
        return button;
    }

    private static Button CreateCreateAccountButton()
    {
        Button button = ViewFactory.CreateButton(
            nameof(Strings.LoginCreateAccountButton),
            ContentWidth,
            SecondaryButtonHeight);
        ButtonSkin.Apply(button, ButtonLooks.Secondary);
        BindEnabledToIdle(button);
        return button;
    }

    private static NoticeBox CreateNotice()
    {
        var notice = new NoticeBox(ContentWidth);
        notice.SetBinding(nameof(NoticeBox.Tone), nameof(LoginViewModel.NoticeTone));
        notice.SetBinding(nameof(NoticeBox.Title), nameof(LoginViewModel.NoticeTitle));
        notice.SetBinding(nameof(NoticeBox.Message), nameof(LoginViewModel.NoticeMessage));
        notice.SetBinding(nameof(IsVisible), nameof(LoginViewModel.HasNotice));
        return notice;
    }

    private static ModalLayer CreateDialogLayer(LoginViewModel viewModel)
    {
        var layer = new ModalLayer { BindingContext = viewModel };
        layer.SetBinding(nameof(ModalLayer.IsOpen), nameof(LoginViewModel.IsDialogOpen));
        layer.AddChild(new LoginDialogView(viewModel));
        return layer;
    }

    private static void BindEnabledToIdle(FrameworkElement control)
    {
        control.SetBinding(nameof(IsEnabled), nameof(LoginViewModel.IsIdle));
    }

    private static Label CreateTwoFactorHint()
    {
        Label hint = ViewFactory.CreateLabel(nameof(Strings.LoginTwoFactorHint), TextStyles.Hint);
        hint.Width = ContentWidth;
        hint.WidthUnits = DimensionUnitType.Absolute;
        return hint;
    }

    private static Button CreateExitButton()
    {
        Button button = ViewFactory.CreateButton(nameof(Strings.CommonExitButton), ExitButtonWidth, ExitButtonHeight);
        ButtonSkin.Apply(button, ButtonLooks.Pill);
        button.Anchor(Gum.Wireframe.Anchor.TopRight);
        button.X = -ScreenMarginX;
        button.Y = ExitButtonMarginY;
        return button;
    }

    private async void OnLogInClick(object? sender, EventArgs e)
    {
        await _viewModel.LogIn();
    }
}
