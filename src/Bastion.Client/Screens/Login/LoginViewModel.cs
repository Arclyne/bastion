using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Accounts;
using Gum.Mvvm;
using log4net;

namespace Bastion.Client.Screens.Login;

public sealed class LoginViewModel : ViewModel
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(LoginViewModel));

    private readonly ILanguageService _languages;
    private readonly IAccountClient _accounts;
    private readonly INavigator _navigator;

    public LoginViewModel(ILanguageService languages, IAccountClient accounts, INavigator navigator)
    {
        ArgumentNullException.ThrowIfNull(languages);
        ArgumentNullException.ThrowIfNull(accounts);
        ArgumentNullException.ThrowIfNull(navigator);

        _languages = languages;
        _accounts = accounts;
        _navigator = navigator;
        Identifier = string.Empty;
        Password = string.Empty;
        NoticeTitle = string.Empty;
        NoticeMessage = string.Empty;
        DialogTitleKey = string.Empty;
        DialogMessageKey = string.Empty;
        Set(languages.CurrentLanguage, nameof(SelectedLanguage));
    }

    public IReadOnlyList<SupportedLanguage> Languages => _languages.Languages;

    public SupportedLanguage SelectedLanguage
    {
        get => Get<SupportedLanguage>();
        set
        {
            if (value is null || !Set(value))
            {
                return;
            }

            _languages.ChangeLanguage(value);
            Password = string.Empty;
            ClearNotice();
        }
    }

    public string Identifier
    {
        get => Get<string>();
        set
        {
            if (Set(value))
            {
                IsIdentifierMissing = false;
            }
        }
    }

    public string Password
    {
        get => Get<string>();
        set
        {
            if (Set(value))
            {
                IsPasswordMissing = false;
            }
        }
    }

    public bool IsPasswordVisible
    {
        get => Get<bool>();
        set => Set(value);
    }

    [DependsOn(nameof(IsPasswordVisible))]
    public bool IsPasswordHidden => !IsPasswordVisible;

    public bool IsIdentifierMissing
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public bool IsPasswordMissing
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public bool IsBusy
    {
        get => Get<bool>();
        private set => Set(value);
    }

    [DependsOn(nameof(IsBusy))]
    public bool IsIdle => !IsBusy;

    [DependsOn(nameof(IsBusy))]
    public string LogInButtonTextKey => IsBusy ? nameof(Strings.LoginButtonBusy) : nameof(Strings.LoginButton);

    public NoticeTone NoticeTone
    {
        get => Get<NoticeTone>();
        private set => Set(value);
    }

    [DependsOn(nameof(NoticeTone))]
    public bool HasNotice => NoticeTone != NoticeTone.None;

    public string NoticeTitle
    {
        get => Get<string>();
        private set => Set(value);
    }

    public string NoticeMessage
    {
        get => Get<string>();
        private set => Set(value);
    }

    public bool IsDialogOpen
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public string DialogTitleKey
    {
        get => Get<string>();
        private set => Set(value);
    }

    public string DialogMessageKey
    {
        get => Get<string>();
        private set => Set(value);
    }

    public bool CanRetry
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public async Task LogIn()
    {
        if (IsBusy || !AreFieldsComplete())
        {
            return;
        }

        ClearNotice();
        IsBusy = true;
        try
        {
            await RequestSession();
        }
        finally
        {
            IsBusy = false;
        }
    }

    public Task RetryLogIn()
    {
        CloseDialog();
        return LogIn();
    }

    public void CloseDialog()
    {
        IsDialogOpen = false;
    }

    private bool AreFieldsComplete()
    {
        IsIdentifierMissing = string.IsNullOrWhiteSpace(Identifier);
        IsPasswordMissing = Password.Length == 0;
        return !IsIdentifierMissing && !IsPasswordMissing;
    }

    private async Task RequestSession()
    {
        try
        {
            var credentials = new LoginCredentials(Identifier.Trim(), Password);
            SignedInSession session = await Task.Run(() => _accounts.LogIn(credentials));
            EnterMainMenu(session);
        }
        catch (InvalidCredentialsException ex)
        {
            _logger.Warn("The server turned down the credentials.", ex);
            Password = string.Empty;
            ShowNotice(NoticeTone.Error, string.Empty, Strings.LoginCredentialsRejected);
        }
        catch (AccountRestrictedException ex)
        {
            _logger.Warn($"The server turned down a restricted account. Restriction={ex.Restriction}", ex);
            ShowRestriction(ex);
        }
        catch (ServerUnreachableException ex)
        {
            _logger.Warn("The server could not be reached.", ex);
            ShowDialog(
                nameof(Strings.DialogServerUnreachableTitle),
                nameof(Strings.DialogServerUnreachableMessage),
                true);
        }
        catch (ConnectionLostException ex)
        {
            _logger.Warn("The connection with the server was lost.", ex);
            ShowDialog(
                nameof(Strings.DialogConnectionLostTitle),
                nameof(Strings.DialogConnectionLostMessage),
                false);
        }
        catch (ServerErrorException ex)
        {
            _logger.Warn("The server could not complete the sign-in.", ex);
            ShowServerErrorDialog();
        }
    }

    private void ShowRestriction(AccountRestrictedException restriction)
    {
        switch (restriction.Restriction)
        {
            case AccountRestriction.Locked:
                ShowNotice(
                    NoticeTone.Warning,
                    FormatWithCurrentCulture(Strings.LoginAccountLockedTitleFormat, restriction.LockMinutes),
                    Strings.LoginAccountLockedMessage);
                break;
            case AccountRestriction.Pending:
                ShowNotice(NoticeTone.Warning, string.Empty, Strings.LoginAccountPending);
                break;
            case AccountRestriction.Suspended:
            case AccountRestriction.Banned:
                ShowNotice(NoticeTone.Error, string.Empty, Strings.LoginAccountSanctioned);
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(restriction),
                    restriction.Restriction,
                    "Unsupported account restriction.");
        }
    }

    private void EnterMainMenu(SignedInSession session)
    {
        ApplyPreferredLanguage(session.PreferredLanguage);
        _navigator.ShowMainMenu(session);
    }

    private void ApplyPreferredLanguage(string preferredLanguage)
    {
        if (string.IsNullOrWhiteSpace(preferredLanguage))
        {
            return;
        }

        SupportedLanguage? language = SupportedLanguages.FindClosest(new CultureInfo(preferredLanguage));
        if (language is not null)
        {
            SelectedLanguage = language;
        }
    }

    private void ShowNotice(NoticeTone tone, string title, string message)
    {
        NoticeTitle = title;
        NoticeMessage = message;
        NoticeTone = tone;
    }

    private void ClearNotice()
    {
        ShowNotice(NoticeTone.None, string.Empty, string.Empty);
    }

    private void ShowServerErrorDialog()
    {
        ShowDialog(nameof(Strings.DialogServerErrorTitle), nameof(Strings.DialogServerErrorMessage), false);
    }

    private void ShowDialog(string titleKey, string messageKey, bool canRetry)
    {
        DialogTitleKey = titleKey;
        DialogMessageKey = messageKey;
        CanRetry = canRetry;
        IsDialogOpen = true;
    }

    private static string FormatWithCurrentCulture(string format, object? argument)
    {
        return string.Format(CultureInfo.CurrentCulture, format, argument);
    }
}
