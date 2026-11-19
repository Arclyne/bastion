using System;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Chat;
using Bastion.Client.Networking.Sessions;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Chat;

public abstract class ModeratedChatViewModel : ChatComposerViewModel
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(ModeratedChatViewModel));

    protected ModeratedChatViewModel(TimeProvider timeProvider)
        : base(timeProvider)
    {
        MuteText = string.Empty;
    }

    public bool IsMuted
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public string MuteText
    {
        get => Get<string>();
        private set => Set(value);
    }

    protected void ShowMuteStatus(ChatMuteStatus muteStatus)
    {
        ArgumentNullException.ThrowIfNull(muteStatus);

        IsMuted = muteStatus.IsMuted;
        MuteText = ChatRejectionText.DescribeMuteStatus(muteStatus, TimeProvider);
    }

    protected async Task DeliverMessage(MessageRowViewModel row, Func<DateTime> send, Action<DateTime> confirmSent)
    {
        ArgumentNullException.ThrowIfNull(row);
        ArgumentNullException.ThrowIfNull(send);
        ArgumentNullException.ThrowIfNull(confirmSent);

        IsSending = true;
        try
        {
            DateTime sentAt = await Task.Run(send);
            confirmSent(sentAt);
        }
        catch (ChatMessageRejectedException ex)
        {
            _logger.Warn($"The server turned down the chat message. Rejection={ex.Rejection}", ex);
            ShowRejection(row, ex.Rejection, ex.MutedUntil);
        }
        catch (InvalidSessionException ex)
        {
            _logger.Warn("The chat message could not be sent because the session is no longer valid.", ex);
            row.Fail(Strings.ChatSendFailed);
        }
        catch (ServerUnreachableException ex)
        {
            ReportSendFailure(row, ex);
        }
        catch (ConnectionLostException ex)
        {
            ReportSendFailure(row, ex);
        }
        catch (ServerErrorException ex)
        {
            ReportSendFailure(row, ex);
        }
        finally
        {
            IsSending = false;
        }
    }

    protected virtual void ShowRejection(MessageRowViewModel row, ChatRejection rejection, DateTime? mutedUntil)
    {
        ArgumentNullException.ThrowIfNull(row);

        row.Fail(ChatRejectionText.Describe(rejection));
        if (rejection == ChatRejection.Muted)
        {
            var muteStatus = new ChatMuteStatus(IsMuted: true, mutedUntil);
            ShowMuteStatus(muteStatus);
        }
    }

    private static void ReportSendFailure(MessageRowViewModel row, Exception cause)
    {
        _logger.Error("The chat message could not be sent.", cause);
        row.Fail(Strings.ChatSendFailed);
    }
}
