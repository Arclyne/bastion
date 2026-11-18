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
    private const float PasswordInputWidth = ContentWidth - RevealButtonWidth - RevealButtonSpacing;
    private const float PrimaryButtonHeight = 52;
    private const float SecondaryButtonHeight = 48;
    private const float ExitButtonWidth = 96;
    private const float ExitButtonHeight = ScreenMetrics.MenuPillHeight;

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
        Panel cardPanel = CreateCard();
        Button exitButton = CreateExitButton();
        Label versionLabel = ViewFactory.CreateVersionLabel();
        AddChild(cardPanel);
        AddChild(exitButton);
        AddChild(versionLabel);
        var serverFailureDialogView = new ServerFailureDialogView(viewModel);
        AddDialog(serverFailureDialogView, viewModel, nameof(LoginViewModel.IsDialogOpen));
    }

    private Panel CreateCard()
    {
        var cardPanel = new Panel
        {
            Width = CardWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = CardPaddingBottom,
            HeightUnits = DimensionUnitType.RelativeToChildren,
        };
        cardPanel.Anchor(Gum.Wireframe.Anchor.Left);
        cardPanel.X = CardX;
        StackPanel contentStackPanel = CreateCardContent();
        cardPanel.AddChild(contentStackPanel);
        CardSkin.Apply(cardPanel.Visual);
        return cardPanel;
    }

    private StackPanel CreateCardContent()
    {
        var contentStackPanel = new StackPanel
        {
            X = CardPaddingX,
            Y = CardPaddingTop,
            Width = ContentWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Spacing = SectionSpacing,
        };
        Panel headerPanel = CreateHeader();
        StackPanel identifierFieldStackPanel = CreateIdentifierField();
        StackPanel passwordFieldStackPanel = CreatePasswordField();
        NoticeBox noticeBox = CreateNotice();
        Button logInButton = CreateLogInButton();
        Label noAccountLabel = ViewFactory.CreateLabel(nameof(Strings.LoginNoAccount), TextStyleCatalog.Divider);
        Button createAccountButton = CreateCreateAccountButton();
        Label twoFactorHintLabel = CreateTwoFactorHint();
        contentStackPanel.AddChild(headerPanel);
        contentStackPanel.AddChild(identifierFieldStackPanel);
        contentStackPanel.AddChild(passwordFieldStackPanel);
        contentStackPanel.AddChild(noticeBox);
        contentStackPanel.AddChild(logInButton);
        contentStackPanel.AddChild(noAccountLabel);
        contentStackPanel.AddChild(createAccountButton);
        contentStackPanel.AddChild(twoFactorHintLabel);
        return contentStackPanel;
    }

    private Panel CreateHeader()
    {
        var brandStackPanel = new StackPanel
        {
            Spacing = HeaderSpacing,
        };
        Label appNameLabel = ViewFactory.CreateLabel(nameof(Strings.AppName), TextStyleCatalog.Title);
        Label taglineLabel = ViewFactory.CreateLabel(nameof(Strings.LoginTagline), TextStyleCatalog.Tagline);
        brandStackPanel.AddChild(appNameLabel);
        brandStackPanel.AddChild(taglineLabel);

        var headerPanel = new Panel
        {
            Width = ContentWidth,
            WidthUnits = DimensionUnitType.Absolute,
            HeightUnits = DimensionUnitType.RelativeToChildren,
        };
        LanguagePicker languagePicker = CreateLanguagePicker();
        headerPanel.AddChild(brandStackPanel);
        headerPanel.AddChild(languagePicker);
        return headerPanel;
    }

    private LanguagePicker CreateLanguagePicker()
    {
        var languagePicker = new LanguagePicker(_languages, LanguagePickerWidth, LanguagePickerHeight);
        languagePicker.SetBinding(nameof(LanguagePicker.Items), nameof(LoginViewModel.Languages));
        languagePicker.SetBinding(nameof(LanguagePicker.SelectedObject), nameof(LoginViewModel.SelectedLanguage));
        BindEnabledToIdle(languagePicker);
        languagePicker.Anchor(Gum.Wireframe.Anchor.TopRight);
        languagePicker.Y = LanguagePickerMarginY;
        return languagePicker;
    }

    private static void BindEnabledToIdle(FrameworkElement control)
    {
        control.SetBinding(nameof(IsEnabled), nameof(LoginViewModel.IsIdle));
    }

    private static StackPanel CreateIdentifierField()
    {
        var inputTextField = new TextField
        {
            Placeholder = nameof(Strings.LoginIdentifierPlaceholder),
            Width = ContentWidth,
            Height = FieldHeight,
        };
        inputTextField.SetBinding(nameof(TextField.Text), nameof(LoginViewModel.Identifier));
        inputTextField.SetBinding(nameof(TextField.HasError), nameof(LoginViewModel.HasIdentifierError));
        BindEnabledToIdle(inputTextField);

        Label missingWarningLabel = ViewFactory.CreateToggledLabel(
            nameof(Strings.LoginIdentifierRequired),
            nameof(LoginViewModel.IsIdentifierMissing),
            TextStyleCatalog.FieldError);
        Label malformedWarningLabel = ViewFactory.CreateToggledLabel(
            nameof(Strings.LoginIdentifierMalformed),
            nameof(LoginViewModel.IsIdentifierMalformed),
            TextStyleCatalog.FieldError);
        return CreateFieldGroup(
            nameof(Strings.LoginIdentifierLabel),
            inputTextField,
            missingWarningLabel,
            malformedWarningLabel);
    }

    private static StackPanel CreateFieldGroup(string captionKey, FrameworkElement input, params Label[] warnings)
    {
        var groupStackPanel = new StackPanel
        {
            Spacing = FieldSpacing,
        };
        Label captionLabel = ViewFactory.CreateLabel(captionKey, TextStyleCatalog.FieldLabel);
        groupStackPanel.AddChild(captionLabel);
        groupStackPanel.AddChild(input);
        foreach (Label warningLabel in warnings)
        {
            groupStackPanel.AddChild(warningLabel);
        }

        return groupStackPanel;
    }

    private static StackPanel CreatePasswordField()
    {
        StackPanel inputsStackPanel = CreatePasswordInputs();
        Label warningLabel = ViewFactory.CreateToggledLabel(
            nameof(Strings.LoginPasswordRequired),
            nameof(LoginViewModel.IsPasswordMissing),
            TextStyleCatalog.FieldError);
        return CreateFieldGroup(nameof(Strings.LoginPasswordLabel), inputsStackPanel, warningLabel);
    }

    private static StackPanel CreatePasswordInputs()
    {
        var maskedInputPasswordField = new PasswordField
        {
            Placeholder = string.Empty,
            Width = PasswordInputWidth,
            Height = FieldHeight,
        };
        maskedInputPasswordField.SetBinding(nameof(PasswordField.Password), nameof(LoginViewModel.Password));
        maskedInputPasswordField.SetBinding(nameof(PasswordField.HasError), nameof(LoginViewModel.IsPasswordMissing));
        maskedInputPasswordField.SetBinding(nameof(IsVisible), nameof(LoginViewModel.IsPasswordMasked));
        BindEnabledToIdle(maskedInputPasswordField);

        var revealedInputTextField = new TextField
        {
            Placeholder = string.Empty,
            Width = PasswordInputWidth,
            Height = FieldHeight,
        };
        revealedInputTextField.SetBinding(nameof(TextField.Text), nameof(LoginViewModel.Password));
        revealedInputTextField.SetBinding(nameof(TextField.HasError), nameof(LoginViewModel.IsPasswordMissing));
        revealedInputTextField.SetBinding(nameof(IsVisible), nameof(LoginViewModel.IsPasswordRevealed));
        BindEnabledToIdle(revealedInputTextField);

        var revealToggleButton = new ToggleButton
        {
            Text = nameof(Strings.LoginShowPasswordButton),
            Width = RevealButtonWidth,
            Height = FieldHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        revealToggleButton.SetBinding(nameof(ToggleButton.IsChecked), nameof(LoginViewModel.IsPasswordRevealed));
        ButtonSkin.CreateAndAttach(revealToggleButton, ButtonLookCatalog.Secondary);
        BindEnabledToIdle(revealToggleButton);

        var rowStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = RevealButtonSpacing,
        };
        rowStackPanel.AddChild(maskedInputPasswordField);
        rowStackPanel.AddChild(revealedInputTextField);
        rowStackPanel.AddChild(revealToggleButton);
        return rowStackPanel;
    }

    private static NoticeBox CreateNotice()
    {
        var noticeBox = new NoticeBox(ContentWidth);
        noticeBox.SetBinding(nameof(NoticeBox.Tone), nameof(LoginViewModel.NoticeTone));
        noticeBox.SetBinding(nameof(NoticeBox.Title), nameof(LoginViewModel.NoticeTitle));
        noticeBox.SetBinding(nameof(NoticeBox.Message), nameof(LoginViewModel.NoticeMessage));
        noticeBox.SetBinding(nameof(IsVisible), nameof(LoginViewModel.HasNotice));
        return noticeBox;
    }

    private Button CreateLogInButton()
    {
        Button button = ViewFactory.CreateButton(nameof(Strings.LoginButton), ContentWidth, PrimaryButtonHeight);
        ButtonSkin.CreateAndAttach(button, ButtonLookCatalog.Primary);
        button.SetBinding(nameof(Button.Text), nameof(LoginViewModel.LogInButtonTextKey));
        button.Click += HandleLogInClick;
        return button;
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

    private static Button CreateCreateAccountButton()
    {
        Button button = ViewFactory.CreateButton(
            nameof(Strings.LoginCreateAccountButton),
            ContentWidth,
            SecondaryButtonHeight);
        ButtonSkin.CreateAndAttach(button, ButtonLookCatalog.Secondary);
        BindEnabledToIdle(button);
        return button;
    }

    private static Label CreateTwoFactorHint()
    {
        Label hintLabel = ViewFactory.CreateLabel(nameof(Strings.LoginTwoFactorHint), TextStyleCatalog.Hint);
        ViewFactory.SetFixedWidth(hintLabel, ContentWidth);
        return hintLabel;
    }

    private static Button CreateExitButton()
    {
        Button button = ViewFactory.CreateButton(nameof(Strings.CommonExitButton), ExitButtonWidth, ExitButtonHeight);
        ButtonSkin.CreateAndAttach(button, ButtonLookCatalog.Pill);
        button.Anchor(Gum.Wireframe.Anchor.TopRight);
        button.X = -ScreenMetrics.MarginX;
        button.Y = ScreenMetrics.MarginY;
        return button;
    }
}
