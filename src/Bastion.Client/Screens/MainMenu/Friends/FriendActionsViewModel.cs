using System;
using System.Globalization;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Friends;
using Bastion.Client.Networking.Sessions;
using Gum.Mvvm;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class FriendActionsViewModel : ViewModel
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(FriendActionsViewModel));

    private readonly SignedInSession _session;
    private readonly IFriendClient _friends;
    private string _friendCode = string.Empty;

    public FriendActionsViewModel(SignedInSession session, IFriendClient friends)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(friends);

        _session = session;
        _friends = friends;
        Nickname = string.Empty;
        DialogTitle = string.Empty;
        DialogMessage = string.Empty;
        ErrorText = string.Empty;
    }

    public event EventHandler<FriendRemovedEventArgs>? FriendRemoved;

    public string Nickname
    {
        get => Get<string>();
        private set => Set(value);
    }

    public bool IsMenuOpen
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public bool IsDialogOpen
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public string DialogTitle
    {
        get => Get<string>();
        private set => Set(value);
    }

    public string DialogMessage
    {
        get => Get<string>();
        private set => Set(value);
    }

    public string ErrorText
    {
        get => Get<string>();
        private set => Set(value);
    }

    [DependsOn(nameof(ErrorText))]
    public bool HasError => ErrorText.Length > 0;

    public bool IsRemoving
    {
        get => Get<bool>();
        private set => Set(value);
    }

    [DependsOn(nameof(IsRemoving))]
    public bool CanConfirm => !IsRemoving;

    public void OpenMenu(FriendRowViewModel row)
    {
        ArgumentNullException.ThrowIfNull(row);

        _friendCode = row.FriendCode;
        Nickname = row.Nickname;
        IsDialogOpen = false;
        IsMenuOpen = true;
    }

    public void AskRemoval()
    {
        DialogTitle = string.Format(CultureInfo.CurrentCulture, Strings.FriendsRemoveTitleFormat, Nickname);
        DialogMessage = string.Format(CultureInfo.CurrentCulture, Strings.FriendsRemoveMessageFormat, Nickname);
        ErrorText = string.Empty;
        IsMenuOpen = false;
        IsDialogOpen = true;
    }

    public void CloseAll()
    {
        IsMenuOpen = false;
        IsDialogOpen = false;
    }

    public async Task ConfirmRemoval()
    {
        if (IsRemoving)
        {
            return;
        }

        string friendCode = _friendCode;
        IsRemoving = true;
        ErrorText = string.Empty;
        try
        {
            await Task.Run(() => _friends.RemoveFriend(_session.Token, friendCode));
            CompleteRemoval(friendCode);
        }
        catch (FriendshipRejectedException ex)
        {
            _logger.Warn("The friend was already gone when removing it.", ex);
            CompleteRemoval(friendCode);
        }
        catch (InvalidSessionException ex)
        {
            ReportFailure(ex);
        }
        catch (ServerUnreachableException ex)
        {
            ReportFailure(ex);
        }
        catch (ConnectionLostException ex)
        {
            ReportFailure(ex);
        }
        catch (ServerErrorException ex)
        {
            ReportFailure(ex);
        }
        finally
        {
            IsRemoving = false;
        }
    }

    private void CompleteRemoval(string friendCode)
    {
        IsDialogOpen = false;
        FriendRemoved?.Invoke(this, new FriendRemovedEventArgs(friendCode));
    }

    private void ReportFailure(Exception ex)
    {
        _logger.Warn("The friend could not be removed.", ex);
        ErrorText = Strings.FriendsRemoveFailed;
    }
}
