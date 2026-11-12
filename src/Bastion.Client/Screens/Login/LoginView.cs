using System;
using Bastion.Client.Controls;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using log4net;

namespace Bastion.Client.Screens.Login;

public sealed class LoginView : ScreenView
{
    private const float CardX = 96;
    private const float CardWidth = 440;
    private const float CardPaddingX = 32;
    private const float CardPaddingTop = 28;
    private const float CardPaddingBottom = 26;
    private const float ContentWidth = CardWidth - (CardPaddingX * HorizontalSideCount);
    private const int HorizontalSideCount = 2;
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

    private static readonly ILog _logger = LogManager.GetLogger(typeof(LoginView));

    private readonly LoginViewModel _viewModel;
    private readonly ILanguageService _languages;

    public LoginView(LoginViewModel viewModel, ILanguageService languages)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(languages);

        _viewModel = viewModel;
        _languages = languages;
        BindingContext = viewModel;
        AddChild(CreateCard());
        AddChild(CreateExitButton());
        AddChild(ViewFactory.CreateVersionLabel());
        AddDialog(new ServerFailureDialogView(viewModel), viewModel, nameof(LoginViewModel.IsDialogOpen));
    }

    private Panel CreateCard()
    {
        var card = new Panel
        {
            Width = CardWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = CardPaddingBottom,
            HeightUnits = DimensionUnitType.RelativeToChildren,
        };
        card.Anchor(Gum.Wireframe.Anchor.Left);
        card.X = CardX;
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

    private Panel CreateHeader()
    {
        var brand = new StackPanel
        {
            Spacing = HeaderSpacing,
        };
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

    private LanguagePicker CreateLanguagePicker()
    {
        var picker = new LanguagePicker(_languages, LanguagePickerWidth, LanguagePickerHeight);
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
        input.SetBinding(nameof(TextField.HasError), nameof(LoginViewModel.HasIdentifierError));
        BindEnabledToIdle(input);

        Label missingWarning = CreateWarning(
            nameof(Strings.LoginIdentifierRequired),
            nameof(LoginViewModel.IsIdentifierMissing));
        Label malformedWarning = CreateWarning(
            nameof(Strings.LoginIdentifierMalformed),
            nameof(LoginViewModel.IsIdentifierMalformed));
        return CreateFieldGroup(nameof(Strings.LoginIdentifierLabel), input, missingWarning, malformedWarning);
    }

    private static Label CreateWarning(string textKey, string visibilityPath)
    {
        Label warning = ViewFactory.CreateLabel(textKey, TextStyles.FieldError);
        warning.SetBinding(nameof(IsVisible), visibilityPath);
        return warning;
    }

    private static StackPanel CreateFieldGroup(string captionKey, FrameworkElement input, params Label[] warnings)
    {
        var group = new StackPanel
        {
            Spacing = FieldSpacing,
        };
        group.AddChild(ViewFactory.CreateLabel(captionKey, TextStyles.FieldLabel));
        group.AddChild(input);
        foreach (Label warning in warnings)
        {
            group.AddChild(warning);
        }

        return group;
    }

    private static StackPanel CreatePasswordField()
    {
        StackPanel inputs = CreatePasswordInputs();
        Label warning = CreateWarning(nameof(Strings.LoginPasswordRequired), nameof(LoginViewModel.IsPasswordMissing));
        return CreateFieldGroup(nameof(Strings.LoginPasswordLabel), inputs, warning);
    }

    private static StackPanel CreatePasswordInputs()
    {
        float inputWidth = ContentWidth - RevealButtonWidth - RevealButtonSpacing;

        var maskedInput = new PasswordField
        {
            Placeholder = string.Empty,
            Width = inputWidth,
            Height = FieldHeight,
        };
        maskedInput.SetBinding(nameof(PasswordField.Password), nameof(LoginViewModel.Password));
        maskedInput.SetBinding(nameof(PasswordField.HasError), nameof(LoginViewModel.IsPasswordMissing));
        maskedInput.SetBinding(nameof(IsVisible), nameof(LoginViewModel.IsPasswordMasked));
        BindEnabledToIdle(maskedInput);

        var revealedInput = new TextField
        {
            Placeholder = string.Empty,
            Width = inputWidth,
            Height = FieldHeight,
        };
        revealedInput.SetBinding(nameof(TextField.Text), nameof(LoginViewModel.Password));
        revealedInput.SetBinding(nameof(TextField.HasError), nameof(LoginViewModel.IsPasswordMissing));
        revealedInput.SetBinding(nameof(IsVisible), nameof(LoginViewModel.IsPasswordRevealed));
        BindEnabledToIdle(revealedInput);

        var revealButton = new ToggleButton
        {
            Text = nameof(Strings.LoginShowPasswordButton),
            Width = RevealButtonWidth,
            Height = FieldHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        revealButton.SetBinding(nameof(ToggleButton.IsChecked), nameof(LoginViewModel.IsPasswordRevealed));
        ButtonSkin.CreateAndAttach(revealButton, ButtonLooks.Secondary);
        BindEnabledToIdle(revealButton);

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = RevealButtonSpacing,
        };
        row.AddChild(maskedInput);
        row.AddChild(revealedInput);
        row.AddChild(revealButton);
        return row;
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

    private Button CreateLogInButton()
    {
        Button button = ViewFactory.CreateButton(nameof(Strings.LoginButton), ContentWidth, PrimaryButtonHeight);
        ButtonSkin.CreateAndAttach(button, ButtonLooks.Primary);
        button.SetBinding(nameof(Button.Text), nameof(LoginViewModel.LogInButtonTextKey));
        button.Click += HandleLogInClick;
        return button;
    }

    private static Button CreateCreateAccountButton()
    {
        Button button = ViewFactory.CreateButton(
            nameof(Strings.LoginCreateAccountButton),
            ContentWidth,
            SecondaryButtonHeight);
        ButtonSkin.CreateAndAttach(button, ButtonLooks.Secondary);
        BindEnabledToIdle(button);
        return button;
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
        ButtonSkin.CreateAndAttach(button, ButtonLooks.Pill);
        button.Anchor(Gum.Wireframe.Anchor.TopRight);
        button.X = -ScreenMetrics.MarginX;
        button.Y = ScreenMetrics.MarginY;
        return button;
    }

    private static void BindEnabledToIdle(FrameworkElement control)
    {
        control.SetBinding(nameof(IsEnabled), nameof(LoginViewModel.IsIdle));
    }

    private async void HandleLogInClick(object? sender, EventArgs e)
    {
        try
        {
            await _viewModel.LogIn();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The login could not be completed because the connection is closed.", ex);
        }
    }
}
