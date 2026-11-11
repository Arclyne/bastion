using System;
using System.Threading.Tasks;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Networking.Sessions;
using Gum.Mvvm;
using log4net;

namespace Bastion.Client.Screens.MainMenu;

public sealed class MainMenuViewModel : ViewModel
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(MainMenuViewModel));

    private readonly LoggedInSession _session;
    private readonly INotificationClient _notifications;

    public MainMenuViewModel(LoggedInSession session, INotificationClient notifications)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(notifications);

        _session = session;
        _notifications = notifications;
        Nickname = session.Nickname;
    }

    public string Nickname
    {
        get => Get<string>();
        private set => Set(value);
    }

    public async Task ConnectNotifications()
    {
        try
        {
            await Task.Run(() => _notifications.Connect(_session.Token));
        }
        catch (InvalidSessionException ex)
        {
            ReportConnectFailure(ex);
        }
        catch (ServerUnreachableException ex)
        {
            ReportConnectFailure(ex);
        }
        catch (ConnectionLostException ex)
        {
            ReportConnectFailure(ex);
        }
        catch (ServerErrorException ex)
        {
            ReportConnectFailure(ex);
        }
    }

    private static void ReportConnectFailure(Exception ex)
    {
        _logger.Warn("The notification channel could not be connected.", ex);
    }
}
