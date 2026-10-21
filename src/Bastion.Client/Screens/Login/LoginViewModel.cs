using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Accounts;
using Gum.Mvvm;

namespace Bastion.Client.Screens.Login;

public sealed class LoginViewModel : ViewModel
{
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
        Set(languages.Current, nameof(SelectedLanguage));
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

            _languages.Change(value);
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

    public Task Retry()
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
        catch (InvalidCredentialsException)
        {
            Password = string.Empty;
            ShowNotice(NoticeTone.Error, string.Empty, Strings.LoginCredentialsRejected);
        }
        catch (AccountRestrictedException ex)
        {
            ShowRestriction(ex);
        }
        catch (ServerUnreachableException)
        {
            ShowDialog(
                nameof(Strings.DialogServerUnreachableTitle),
                nameof(Strings.DialogServerUnreachableMessage),
                true);
        }
        catch (ConnectionLostException)
        {
            ShowDialog(
                nameof(Strings.DialogConnectionLostTitle),
                nameof(Strings.DialogConnectionLostMessage),
                false);
        }
        catch (ServerErrorException)
        {
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
                    Format(Strings.LoginAccountLockedTitleFormat, restriction.LockMinutes),
                    Strings.LoginAccountLockedMessage);
                break;
            case AccountRestriction.Pending:
                ShowNotice(NoticeTone.Warning, string.Empty, Strings.LoginAccountPending);
                break;
            default:
                ShowNotice(NoticeTone.Error, string.Empty, Strings.LoginAccountSanctioned);
                break;
        }
    }

    private void EnterMainMenu(SignedInSession session)
    {
        ApplyPreferredLanguage(session.PreferredLanguage);
        _navigator.ShowMainMenu(session.Nickname);
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

    private static string Format(string format, object? argument)
    {
        return string.Format(CultureInfo.CurrentCulture, format, argument);
    }
}
