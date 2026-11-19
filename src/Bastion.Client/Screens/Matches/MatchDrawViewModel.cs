using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Matches;
using Bastion.Client.Networking.Matchmaking;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Networking.Sessions;
using Bastion.Domain.Matches;
using Gum.Mvvm;
using log4net;

namespace Bastion.Client.Screens.Matches;

public sealed class MatchDrawViewModel : ViewModel, IDisposable
{
    public const int ToastSeconds = 3;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(MatchDrawViewModel));

    private readonly JoinedMatch _joinedMatch;
    private readonly MatchPlayViewModel _matchPlay;
    private readonly IMatchClient _matches;
    private readonly TimeProvider _timeProvider;
    private readonly SynchronizationContext? _uiContext;
    private readonly byte _ownTurnOrder;
    private readonly string _ownNickname;
    private ITimer? _toastTimer;

    public MatchDrawViewModel(
        JoinedMatch joinedMatch,
        MatchPlayViewModel matchPlayViewModel,
        MatchConnectionServices services)
    {
        ArgumentNullException.ThrowIfNull(joinedMatch);
        ArgumentNullException.ThrowIfNull(matchPlayViewModel);
        ArgumentNullException.ThrowIfNull(services);

        _joinedMatch = joinedMatch;
        _matchPlay = matchPlayViewModel;
        _matches = services.Matches;
        _timeProvider = services.TimeProvider;
        _uiContext = SynchronizationContext.Current;
        _ownTurnOrder = joinedMatch.GetOwnTurnOrder();
        _ownNickname = joinedMatch.Session.Nickname;
        IsAvailable = joinedMatch.FoundMatch.Participants.Count == Match.TwoPlayerCount;
        RivalOfferTitleText = string.Empty;
        ToastText = string.Empty;
        _matches.DrawOffered += HandleDrawOffered;
        _matches.DrawRejected += HandleDrawRejected;
        _matches.DrawOfferExpired += HandleDrawOfferExpired;
        _matches.PawnMoved += HandlePawnMoved;
        _matches.WallPlaced += HandleWallPlaced;
        _matchPlay.Rejoined += HandleRejoined;
        _matchPlay.MatchEndReceived += HandleMatchEndReceived;
        _matchPlay.OwnParticipationEnded += HandleOwnParticipationEnded;
    }

    public bool IsAvailable
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public bool IsOwnOfferPending
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public int MovesLeft
    {
        get => Get<int>();
        private set => Set(value);
    }

    public bool IsSending
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public bool IsMatchOver
    {
        get => Get<bool>();
        private set => Set(value);
    }

    [DependsOn(nameof(IsAvailable))]
    [DependsOn(nameof(IsMatchOver))]
    public bool IsOfferButtonShown => IsAvailable && !IsMatchOver;

    [DependsOn(nameof(IsAvailable))]
    [DependsOn(nameof(IsOwnOfferPending))]
    [DependsOn(nameof(MovesLeft))]
    [DependsOn(nameof(IsSending))]
    [DependsOn(nameof(IsMatchOver))]
    public bool CanOfferDraw => IsAvailable && !IsOwnOfferPending && MovesLeft == 0 && !IsSending && !IsMatchOver;

    [DependsOn(nameof(IsOwnOfferPending))]
    [DependsOn(nameof(MovesLeft))]
    public string OfferButtonText => DescribeOfferButton();

    public bool HasRivalOffer
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public string RivalOfferTitleText
    {
        get => Get<string>();
        private set => Set(value);
    }

    [DependsOn(nameof(IsSending))]
    public bool CanAnswer => !IsSending;

    public string ToastText
    {
        get => Get<string>();
        private set => Set(value);
    }

    [DependsOn(nameof(ToastText))]
    public bool HasToast => ToastText.Length > 0;

    public event EventHandler? OwnOfferChanged;

    public async Task OfferDraw()
    {
        if (!CanOfferDraw)
        {
            return;
        }

        IsSending = true;
        try
        {
            await Task.Run(OfferDrawInJoinedMatch);
            SetOwnOfferPending(isPending: true);
        }
        catch (DrawOfferRejectedException ex)
        {
            ReceiveRefusal(ex);
        }
        catch (MatchNotJoinedException ex)
        {
            _logger.Warn("The server says the player is not in that match any more.", ex);
        }
        catch (InvalidSessionException ex)
        {
            ReportFailure(ex, Strings.DrawOfferFailed);
        }
        catch (ServerUnreachableException ex)
        {
            ReportFailure(ex, Strings.DrawOfferFailed);
        }
        catch (ConnectionLostException ex)
        {
            ReportFailure(ex, Strings.DrawOfferFailed);
        }
        catch (ServerErrorException ex)
        {
            ReportFailure(ex, Strings.DrawOfferFailed);
        }
        finally
        {
            IsSending = false;
        }
    }

    public Task AcceptDraw()
    {
        return AnswerDraw(isAccepted: true);
    }

    public Task RejectDraw()
    {
        return AnswerDraw(isAccepted: false);
    }

    public void Dispose()
    {
        Interlocked.Exchange(ref _toastTimer, value: null)?.Dispose();
        _matches.DrawOffered -= HandleDrawOffered;
        _matches.DrawRejected -= HandleDrawRejected;
        _matches.DrawOfferExpired -= HandleDrawOfferExpired;
        _matches.PawnMoved -= HandlePawnMoved;
        _matches.WallPlaced -= HandleWallPlaced;
        _matchPlay.Rejoined -= HandleRejoined;
        _matchPlay.MatchEndReceived -= HandleMatchEndReceived;
        _matchPlay.OwnParticipationEnded -= HandleOwnParticipationEnded;
    }

    private string DescribeOfferButton()
    {
        if (IsOwnOfferPending)
        {
            return Strings.DrawOfferedButton;
        }

        if (MovesLeft == 1)
        {
            return Strings.DrawOfferInOneMove;
        }

        if (MovesLeft > 1)
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.DrawOfferInMovesFormat, MovesLeft);
        }

        return Strings.DrawOfferButton;
    }

    private void OfferDrawInJoinedMatch()
    {
        _matches.OfferDraw(_joinedMatch.Session.Token, _joinedMatch.FoundMatch.MatchId);
    }

    private void SetOwnOfferPending(bool isPending)
    {
        IsOwnOfferPending = isPending;
        OwnOfferChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ReceiveRefusal(DrawOfferRejectedException refusal)
    {
        _logger.Info($"The server turned down the draw offer. Refusal={refusal.Refusal}; MovesLeft={refusal.MovesLeft}");
        switch (refusal.Refusal)
        {
            case DrawOfferRefusal.TooSoon:
                MovesLeft = refusal.MovesLeft;
                break;
            case DrawOfferRefusal.AlreadyOffered:
                SetOwnOfferPending(isPending: true);
                break;
            default:
                IsAvailable = false;
                break;
        }
    }

    private void ReportFailure(Exception cause, string text)
    {
        _logger.Error("The draw request could not be sent.", cause);
        ShowToast(text);
    }

    private void ShowToast(string text)
    {
        Interlocked.Exchange(ref _toastTimer, value: null)?.Dispose();
        ToastText = text;
        var duration = TimeSpan.FromSeconds(ToastSeconds);
        _toastTimer = _timeProvider.CreateTimer(HandleToastTimer, state: null, duration, Timeout.InfiniteTimeSpan);
    }

    private void HandleToastTimer(object? state)
    {
        UiThread.Run(_uiContext, HideToast);
    }

    private void HideToast()
    {
        Interlocked.Exchange(ref _toastTimer, value: null)?.Dispose();
        ToastText = string.Empty;
    }

    private async Task AnswerDraw(bool isAccepted)
    {
        if (!HasRivalOffer || IsSending)
        {
            return;
        }

        IsSending = true;
        try
        {
            await Task.Run(() => RespondInJoinedMatch(isAccepted));
            HasRivalOffer = false;
        }
        catch (DrawNotPendingException ex)
        {
            _logger.Info("The draw offer was no longer pending when it was answered.", ex);
            HasRivalOffer = false;
        }
        catch (MatchNotJoinedException ex)
        {
            _logger.Warn("The server says the player is not in that match any more.", ex);
            HasRivalOffer = false;
        }
        catch (InvalidSessionException ex)
        {
            ReportFailure(ex, Strings.DrawAnswerFailed);
        }
        catch (ServerUnreachableException ex)
        {
            ReportFailure(ex, Strings.DrawAnswerFailed);
        }
        catch (ConnectionLostException ex)
        {
            ReportFailure(ex, Strings.DrawAnswerFailed);
        }
        catch (ServerErrorException ex)
        {
            ReportFailure(ex, Strings.DrawAnswerFailed);
        }
        finally
        {
            IsSending = false;
        }
    }

    private void RespondInJoinedMatch(bool isAccepted)
    {
        _matches.RespondToDraw(_joinedMatch.Session.Token, _joinedMatch.FoundMatch.MatchId, isAccepted);
    }

    private void HandleDrawOffered(object? sender, DrawEventArgs e)
    {
        if (!_joinedMatch.IsJoinedMatch(e.MatchId) || e.OffererTurnOrder == _ownTurnOrder)
        {
            return;
        }

        Action showRivalOffer = () => ShowRivalOffer(e.OffererTurnOrder);
        UiThread.Run(_uiContext, showRivalOffer);
    }

    private void ShowRivalOffer(byte offererTurnOrder)
    {
        string nickname = FindNickname(offererTurnOrder);
        RivalOfferTitleText = string.Format(CultureInfo.CurrentCulture, Strings.DrawOfferTitleFormat, nickname);
        HasRivalOffer = true;
    }

    private string FindNickname(byte turnOrder)
    {
        FoundMatchParticipant? participant = _joinedMatch.FoundMatch.Participants.FirstOrDefault(
            candidate => candidate.TurnOrder == turnOrder);
        if (participant is null)
        {
            return string.Empty;
        }

        return participant.Nickname;
    }

    private void HandleDrawRejected(object? sender, DrawEventArgs e)
    {
        if (!_joinedMatch.IsJoinedMatch(e.MatchId))
        {
            return;
        }

        UiThread.Run(_uiContext, ReceiveRejection);
    }

    private void ReceiveRejection()
    {
        CloseOwnOffer();
        string rivalNickname = FindRivalNickname();
        ShowToast(string.Format(CultureInfo.CurrentCulture, Strings.DrawRejectedFormat, rivalNickname));
    }

    private void CloseOwnOffer()
    {
        SetOwnOfferPending(isPending: false);
        MovesLeft = MatchDrawPolicy.OwnMovesBetweenOffers;
    }

    private string FindRivalNickname()
    {
        FoundMatchParticipant? rival = _joinedMatch.FoundMatch.Participants.FirstOrDefault(
            participant => participant.TurnOrder != _ownTurnOrder);
        if (rival is null)
        {
            return string.Empty;
        }

        return rival.Nickname;
    }

    private void HandleDrawOfferExpired(object? sender, DrawEventArgs e)
    {
        if (!_joinedMatch.IsJoinedMatch(e.MatchId))
        {
            return;
        }

        UiThread.Run(_uiContext, ReceiveExpiry);
    }

    private void ReceiveExpiry()
    {
        HasRivalOffer = false;
        if (!IsOwnOfferPending)
        {
            return;
        }

        CloseOwnOffer();
        string rivalNickname = FindRivalNickname();
        ShowToast(string.Format(CultureInfo.CurrentCulture, Strings.DrawExpiredFormat, rivalNickname));
    }

    private void HandlePawnMoved(object? sender, PawnMovedEventArgs e)
    {
        if (!_joinedMatch.IsJoinedMatch(e.MatchId))
        {
            return;
        }

        Action countMove = () => CountMove(e.MoverNickname);
        UiThread.Run(_uiContext, countMove);
    }

    private void CountMove(string moverNickname)
    {
        if (string.Equals(moverNickname, _ownNickname, StringComparison.Ordinal) && MovesLeft > 0)
        {
            MovesLeft--;
        }
    }

    private void HandleWallPlaced(object? sender, WallPlacedEventArgs e)
    {
        if (!_joinedMatch.IsJoinedMatch(e.MatchId))
        {
            return;
        }

        Action countMove = () => CountMove(e.PlacerNickname);
        UiThread.Run(_uiContext, countMove);
    }

    private void HandleRejoined(object? sender, MatchRejoinedEventArgs e)
    {
        RejoinedMatch rejoinedMatch = e.RejoinedMatch;
        MovesLeft = rejoinedMatch.DrawMovesLeft;
        byte offererTurnOrder = rejoinedMatch.PendingDrawOffererTurnOrder;
        SetOwnOfferPending(offererTurnOrder == _ownTurnOrder);
        HasRivalOffer = false;
        if (offererTurnOrder != 0 && offererTurnOrder != _ownTurnOrder)
        {
            ShowRivalOffer(offererTurnOrder);
        }
    }

    private void HandleMatchEndReceived(object? sender, MatchEndedEventArgs e)
    {
        CloseEverything();
    }

    private void CloseEverything()
    {
        IsMatchOver = true;
        HasRivalOffer = false;
        SetOwnOfferPending(isPending: false);
    }

    private void HandleOwnParticipationEnded(object? sender, ParticipationEndedEventArgs e)
    {
        CloseEverything();
    }
}
