using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Networking.Rooms;
using Bastion.Client.Networking.Sessions;
using Bastion.Domain.Common;
using Gum.Mvvm;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Rooms;

public sealed class PrivateRoomViewModel : ViewModel, IDisposable
{
    public const int CopiedSeconds = 2;
    public const int MaximumSeatCount = 4;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(PrivateRoomViewModel));

    private readonly LoggedInSession _session;
    private readonly IRoomClient _rooms;
    private readonly IClipboard _clipboard;
    private readonly TimeProvider _timeProvider;
    private readonly SynchronizationContext? _uiContext;
    private PrivateRoom? _room;
    private ITimer? _copiedTimer;
    private string _pendingKickNickname;

    public PrivateRoomViewModel(LoggedInSession session, PrivateRoomServices services)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(services);

        _session = session;
        _rooms = services.Rooms;
        _clipboard = services.Clipboard;
        _timeProvider = services.TimeProvider;
        _uiContext = SynchronizationContext.Current;
        _pendingKickNickname = string.Empty;
        Sheet = new RoomSettingsSheetViewModel();
        JoinDialog = new JoinRoomDialogViewModel(services.TimeProvider);
        IEnumerable<RoomSeatViewModel> seats = Enumerable.Range(0, MaximumSeatCount)
            .Select(seatIndex => new RoomSeatViewModel(seatIndex));
        Seats = [.. seats];
        Code = string.Empty;
        HostNickname = string.Empty;
        ModeText = string.Empty;
        WallsText = string.Empty;
        ClockText = string.Empty;
        PlayersText = string.Empty;
        FooterText = string.Empty;
        ErrorText = string.Empty;
        NoticeText = string.Empty;
        SettingsNoticeText = string.Empty;
        KickTitleText = string.Empty;
        _rooms.RoomUpdated += HandleRoomUpdated;
        _rooms.RoomClosed += HandleRoomClosed;
        _rooms.Disconnected += HandleDisconnected;
    }

    public RoomSettingsSheetViewModel Sheet { get; }

    public JoinRoomDialogViewModel JoinDialog { get; }

    public IReadOnlyList<RoomSeatViewModel> Seats { get; }

    public bool IsInRoom
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public bool IsHost
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public string Code
    {
        get => Get<string>();
        private set => Set(value);
    }

    public string HostNickname
    {
        get => Get<string>();
        private set => Set(value);
    }

    public string ModeText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public string WallsText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public string ClockText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public bool IsModeChanged
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public bool IsWallsChanged
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public bool IsClockChanged
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public int SeatCount
    {
        get => Get<int>();
        private set => Set(value);
    }

    [DependsOn(nameof(SeatCount))]
    public bool IsFourSeatLayout => SeatCount > PrivateRoomModes.GetSeatCount(PrivateRoomGameMode.Classic);

    [DependsOn(nameof(SeatCount))]
    public bool IsTwoSeatLayout => !IsFourSeatLayout;

    public string PlayersText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public bool IsOwnReady
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public bool IsRoomFilledAndReady
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public bool IsStarting
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public bool IsSending
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public string FooterText
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

    [DependsOn(nameof(IsHost))]
    public bool IsStartShown => IsHost;

    [DependsOn(nameof(IsHost))]
    [DependsOn(nameof(IsRoomFilledAndReady))]
    [DependsOn(nameof(IsStarting))]
    public bool CanStart => IsHost && IsRoomFilledAndReady && !IsStarting;

    [DependsOn(nameof(IsStarting))]
    public string StartButtonKey => IsStarting ? nameof(Strings.RoomStarting) : nameof(Strings.RoomStartMatch);

    [DependsOn(nameof(IsHost))]
    [DependsOn(nameof(IsOwnReady))]
    public bool IsReadyShown => !IsHost && !IsOwnReady;

    [DependsOn(nameof(IsHost))]
    [DependsOn(nameof(IsOwnReady))]
    public bool IsNotReadyShown => !IsHost && IsOwnReady;

    [DependsOn(nameof(IsSending))]
    public bool CanSend => !IsSending;

    [DependsOn(nameof(IsFooterPositive))]
    public bool IsFooterNeutral => !IsFooterPositive;

    public bool IsFooterPositive
    {
        get => Get<bool>();
        private set => Set(value);
    }

    [DependsOn(nameof(IsHost))]
    public string LeaveButtonKey => IsHost ? nameof(Strings.RoomCloseRoom) : nameof(Strings.RoomLeaveRoom);

    public bool IsCodeCopied
    {
        get => Get<bool>();
        private set => Set(value);
    }

    [DependsOn(nameof(IsCodeCopied))]
    public string CopyButtonKey => IsCodeCopied ? nameof(Strings.RoomCodeCopied) : nameof(Strings.RoomCopyCode);

    public string NoticeText
    {
        get => Get<string>();
        private set => Set(value);
    }

    [DependsOn(nameof(NoticeText))]
    public bool HasNotice => NoticeText.Length > 0;

    public string SettingsNoticeText
    {
        get => Get<string>();
        private set => Set(value);
    }

    [DependsOn(nameof(SettingsNoticeText))]
    [DependsOn(nameof(IsInRoom))]
    public bool HasSettingsNotice => IsInRoom && SettingsNoticeText.Length > 0;

    public bool IsKickDialogOpen
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public string KickTitleText
    {
        get => Get<string>();
        private set => Set(value);
    }

    [DependsOn(nameof(IsInRoom))]
    public bool IsMenuShown => !IsInRoom;

    public void OpenCreateSheet()
    {
        if (IsInRoom)
        {
            return;
        }

        NoticeText = string.Empty;
        Sheet.OpenToCreate();
    }

    public void OpenChangeSheet()
    {
        if (_room is null || !IsHost)
        {
            return;
        }

        Sheet.OpenToEdit(_room.Options, _room.Members.Count);
    }

    public async Task ConfirmSheet()
    {
        if (!Sheet.CanConfirm)
        {
            return;
        }

        PrivateRoomOptions options = Sheet.GetOptions();
        Func<PrivateRoom> request = Sheet.IsEditing
            ? () => _rooms.ChangeOptions(_session.Token, options)
            : () => _rooms.CreateRoom(_session.Token, options);
        Sheet.IsSending = true;
        Lookup<PrivateRoom> answer = await SendRoomRequest(request, Sheet.ShowError);
        Sheet.IsSending = false;
        if (!answer.IsFound)
        {
            return;
        }

        Sheet.Close();
        ShowRoom(answer.GetValue());
    }

    public void OpenJoinDialog()
    {
        if (IsInRoom)
        {
            return;
        }

        NoticeText = string.Empty;
        JoinDialog.Open();
    }

    public async Task JoinWithCode()
    {
        if (!JoinDialog.CanEnter || !JoinDialog.AcceptCode())
        {
            return;
        }

        string code = JoinDialog.CodeText;
        JoinDialog.IsSending = true;
        try
        {
            PrivateRoom room = await Task.Run(() => _rooms.JoinRoom(_session.Token, code));
            JoinDialog.Close();
            ShowRoom(room);
        }
        catch (RoomRejectedException ex)
        {
            _logger.Info($"The server turned down the room code. Rejection={ex.Rejection}");
            JoinDialog.ShowRejection(ex.Rejection, ex.RetryDelay);
        }
        catch (InvalidSessionException ex)
        {
            ReportFailure(ex, JoinDialog.ShowError);
        }
        catch (ServerUnreachableException ex)
        {
            ReportFailure(ex, JoinDialog.ShowError);
        }
        catch (ConnectionLostException ex)
        {
            ReportFailure(ex, JoinDialog.ShowError);
        }
        catch (ServerErrorException ex)
        {
            ReportFailure(ex, JoinDialog.ShowError);
        }
        finally
        {
            JoinDialog.IsSending = false;
        }
    }

    public async Task ToggleReady()
    {
        if (!IsInRoom || IsHost || IsSending)
        {
            return;
        }

        bool isReady = !IsOwnReady;
        IsSending = true;
        Lookup<PrivateRoom> answer = await SendRoomRequest(() => _rooms.SetReady(_session.Token, isReady), ShowError);
        IsSending = false;
        if (answer.IsFound)
        {
            ShowRoom(answer.GetValue());
        }
    }

    public async Task StartMatch()
    {
        if (!CanStart)
        {
            return;
        }

        ErrorText = string.Empty;
        IsStarting = true;
        try
        {
            await Task.Run(() => _rooms.StartMatch(_session.Token));
        }
        catch (RoomRejectedException ex)
        {
            _logger.Info($"The server did not start the room match. Rejection={ex.Rejection}");
            ErrorText = RoomRejectionText.Describe(ex.Rejection);
        }
        catch (InvalidSessionException ex)
        {
            ReportFailure(ex, ShowError);
        }
        catch (ServerUnreachableException ex)
        {
            ReportFailure(ex, ShowError);
        }
        catch (ConnectionLostException ex)
        {
            ReportFailure(ex, ShowError);
        }
        catch (ServerErrorException ex)
        {
            ReportFailure(ex, ShowError);
        }
        finally
        {
            IsStarting = false;
        }
    }

    public void AskToKick(string nickname)
    {
        ArgumentNullException.ThrowIfNull(nickname);

        if (!IsHost || nickname.Length == 0)
        {
            return;
        }

        _pendingKickNickname = nickname;
        KickTitleText = string.Format(CultureInfo.CurrentCulture, Strings.RoomKickTitleFormat, nickname);
        IsKickDialogOpen = true;
    }

    public void CancelKick()
    {
        _pendingKickNickname = string.Empty;
        IsKickDialogOpen = false;
    }

    public async Task ConfirmKick()
    {
        if (!IsKickDialogOpen || IsSending)
        {
            return;
        }

        string nickname = _pendingKickNickname;
        IsSending = true;
        Lookup<PrivateRoom> answer = await SendRoomRequest(
            () => _rooms.KickPlayer(_session.Token, nickname),
            ShowError);
        IsSending = false;
        CancelKick();
        if (answer.IsFound)
        {
            ShowRoom(answer.GetValue());
        }
    }

    public async Task Leave()
    {
        if (!IsInRoom)
        {
            return;
        }

        ClearRoom();
        try
        {
            await Task.Run(() => _rooms.LeaveRoom(_session.Token));
        }
        catch (ServerUnreachableException ex)
        {
            _logger.Warn("The server did not hear that the player left the room.", ex);
        }
        catch (ConnectionLostException ex)
        {
            _logger.Warn("The server did not hear that the player left the room.", ex);
        }
        catch (ServerErrorException ex)
        {
            _logger.Warn("The server did not hear that the player left the room.", ex);
        }
    }

    public void CopyCode()
    {
        if (!_clipboard.CanCopyText)
        {
            _logger.Warn("The room code could not be copied because there is no clipboard.");
            return;
        }

        _clipboard.CopyText(Code);
        Interlocked.Exchange(ref _copiedTimer, value: null)?.Dispose();
        IsCodeCopied = true;
        var duration = TimeSpan.FromSeconds(CopiedSeconds);
        _copiedTimer = _timeProvider.CreateTimer(HandleCopiedTimer, state: null, duration, Timeout.InfiniteTimeSpan);
    }

    public void DismissNotice()
    {
        NoticeText = string.Empty;
    }

    public void DismissSettingsNotice()
    {
        SettingsNoticeText = string.Empty;
    }

    public bool CloseOpenDialog()
    {
        if (IsKickDialogOpen)
        {
            CancelKick();
            return true;
        }

        if (Sheet.IsOpen)
        {
            Sheet.Close();
            return true;
        }

        if (JoinDialog.IsOpen)
        {
            JoinDialog.Close();
            return true;
        }

        return false;
    }

    public async Task RestoreOwnRoom()
    {
        try
        {
            OwnRoom ownRoom = await Task.Run(() => _rooms.FindOwnRoom(_session.Token));
            if (ownRoom.IsInRoom)
            {
                ShowRoom(ownRoom.GetRoom());
            }
        }
        catch (InvalidSessionException ex)
        {
            _logger.Warn("The own room could not be looked up because the session is not valid.", ex);
        }
        catch (ServerUnreachableException ex)
        {
            _logger.Warn("The own room could not be looked up.", ex);
        }
        catch (ConnectionLostException ex)
        {
            _logger.Warn("The own room could not be looked up.", ex);
        }
        catch (ServerErrorException ex)
        {
            _logger.Warn("The own room could not be looked up.", ex);
        }
    }

    public void Dispose()
    {
        Interlocked.Exchange(ref _copiedTimer, value: null)?.Dispose();
        JoinDialog.Dispose();
        _rooms.RoomUpdated -= HandleRoomUpdated;
        _rooms.RoomClosed -= HandleRoomClosed;
        _rooms.Disconnected -= HandleDisconnected;
    }

    private static async Task<Lookup<PrivateRoom>> SendRoomRequest(Func<PrivateRoom> request, Action<string> reportError)
    {
        try
        {
            PrivateRoom room = await Task.Run(request);
            return Lookup.Found(room);
        }
        catch (RoomRejectedException ex)
        {
            _logger.Info($"The server turned down the room request. Rejection={ex.Rejection}");
            reportError(RoomRejectionText.Describe(ex.Rejection));
        }
        catch (InvalidSessionException ex)
        {
            ReportFailure(ex, reportError);
        }
        catch (ServerUnreachableException ex)
        {
            ReportFailure(ex, reportError);
        }
        catch (ConnectionLostException ex)
        {
            ReportFailure(ex, reportError);
        }
        catch (ServerErrorException ex)
        {
            ReportFailure(ex, reportError);
        }

        return Lookup.Missing<PrivateRoom>("The room request failed.");
    }

    private static void ReportFailure(Exception cause, Action<string> reportError)
    {
        _logger.Error("The room request could not be completed.", cause);
        reportError(Strings.RoomRequestFailed);
    }

    private void ShowRoom(PrivateRoom room)
    {
        PrivateRoomMember host = room.Members.First(member => member.IsHost);
        PrivateRoomMember? own = room.Members.FirstOrDefault(member => member.Nickname == _session.Nickname);
        _room = room;
        Code = room.Code;
        HostNickname = host.Nickname;
        IsHost = own?.IsHost ?? false;
        IsOwnReady = own?.IsReady ?? false;
        SeatCount = room.SeatCount;
        ShowSettings(room.Options);
        ShowSeats(room);
        ShowFooter(room);
        ErrorText = string.Empty;
        IsInRoom = true;
        if (IsOwnReady && !IsHost)
        {
            ForgetChangedSettings();
        }
    }

    private void ShowSettings(PrivateRoomOptions options)
    {
        string modeName = PrivateRoomModes.GetName(options.GameMode);
        int seatCount = PrivateRoomModes.GetSeatCount(options.GameMode);
        int boardSize = PrivateRoomModes.GetBoardSize(options.GameMode);
        ModeText = string.Format(
            CultureInfo.CurrentCulture,
            Strings.RoomModeValueFormat,
            modeName,
            seatCount,
            boardSize);
        WallsText = options.WallsPerPlayer.ToString(CultureInfo.CurrentCulture);
        ClockText = string.Format(CultureInfo.CurrentCulture, Strings.RoomClockFormat, options.ClockMinutes);
    }

    private void ShowSeats(PrivateRoom room)
    {
        foreach (RoomSeatViewModel seat in Seats)
        {
            PrivateRoomMember? member = room.Members.FirstOrDefault(candidate => candidate.SeatIndex == seat.SeatIndex);
            if (member is null)
            {
                seat.ShowFree();
                continue;
            }

            seat.ShowMember(member, _session.Nickname, IsHost);
        }

        PlayersText = string.Format(
            CultureInfo.CurrentCulture,
            Strings.RoomPlayersFormat,
            room.Members.Count,
            room.SeatCount);
    }

    private void ShowFooter(PrivateRoom room)
    {
        int freeSeatCount = room.SeatCount - room.Members.Count;
        List<PrivateRoomMember> waitingMembers = [.. room.Members.Where(member => !member.IsReady)];
        IsRoomFilledAndReady = freeSeatCount == 0 && waitingMembers.Count == 0;
        IsFooterPositive = IsHost ? IsRoomFilledAndReady : IsOwnReady;
        FooterText = IsHost ? DescribeHostWait(freeSeatCount, waitingMembers) : DescribeGuestWait();
    }

    private static string DescribeHostWait(int freeSeatCount, List<PrivateRoomMember> waitingMembers)
    {
        if (freeSeatCount == 1)
        {
            return Strings.RoomWaitingOneSeat;
        }

        if (freeSeatCount > 1)
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.RoomWaitingSeatsFormat, freeSeatCount);
        }

        if (waitingMembers.Count == 1)
        {
            string nickname = waitingMembers[0].Nickname;
            return string.Format(CultureInfo.CurrentCulture, Strings.RoomWaitingReadyFormat, nickname);
        }

        return waitingMembers.Count > 1 ? Strings.RoomWaitingManyReady : Strings.RoomAllReady;
    }

    private string DescribeGuestWait()
    {
        if (!IsOwnReady)
        {
            return Strings.RoomReadyHint;
        }

        return string.Format(CultureInfo.CurrentCulture, Strings.RoomReadyWaitingFormat, HostNickname);
    }

    private void ForgetChangedSettings()
    {
        IsModeChanged = false;
        IsWallsChanged = false;
        IsClockChanged = false;
        SettingsNoticeText = string.Empty;
    }

    private void ShowError(string text)
    {
        ErrorText = text;
    }

    private void HandleCopiedTimer(object? state)
    {
        UiThread.Run(_uiContext, HideCopied);
    }

    private void HideCopied()
    {
        Interlocked.Exchange(ref _copiedTimer, value: null)?.Dispose();
        IsCodeCopied = false;
    }

    private void ClearRoom()
    {
        _room = null;
        IsInRoom = false;
        IsHost = false;
        IsOwnReady = false;
        IsKickDialogOpen = false;
        _pendingKickNickname = string.Empty;
        ErrorText = string.Empty;
        ForgetChangedSettings();
        Sheet.Close();
    }

    private void HandleRoomUpdated(object? sender, RoomUpdatedEventArgs e)
    {
        Action applyUpdate = () => ApplyUpdate(e.Room);
        UiThread.Run(_uiContext, applyUpdate);
    }

    private void ApplyUpdate(PrivateRoom room)
    {
        if (_room is null || room.Code != _room.Code)
        {
            return;
        }

        if (!IsHost)
        {
            MarkChangedSettings(_room.Options, room.Options);
        }

        ShowRoom(room);
    }

    private void MarkChangedSettings(PrivateRoomOptions previous, PrivateRoomOptions current)
    {
        if (previous == current)
        {
            return;
        }

        IsModeChanged = IsModeChanged || previous.GameMode != current.GameMode;
        IsWallsChanged = IsWallsChanged || previous.WallsPerPlayer != current.WallsPerPlayer;
        IsClockChanged = IsClockChanged || previous.ClockMinutes != current.ClockMinutes;
        SettingsNoticeText = string.Format(CultureInfo.CurrentCulture, Strings.RoomChangedNoticeFormat, HostNickname);
    }

    private void HandleRoomClosed(object? sender, RoomClosedEventArgs e)
    {
        Action closeRoom = () => CloseRoom(e);
        UiThread.Run(_uiContext, closeRoom);
    }

    private void CloseRoom(RoomClosedEventArgs closing)
    {
        if (_room is null || closing.Code != _room.Code)
        {
            return;
        }

        string hostNickname = HostNickname;
        ClearRoom();
        string format = closing.Reason == RoomCloseReason.Kicked
            ? Strings.RoomKickedNoticeFormat
            : Strings.RoomClosedNoticeFormat;
        NoticeText = string.Format(CultureInfo.CurrentCulture, format, hostNickname);
    }

    private void HandleDisconnected(object? sender, EventArgs e)
    {
        UiThread.Run(_uiContext, LeaveAfterDisconnection);
    }

    private void LeaveAfterDisconnection()
    {
        if (!IsInRoom)
        {
            return;
        }

        ClearRoom();
        NoticeText = Strings.RoomLostConnectionNotice;
    }
}
