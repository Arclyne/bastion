using System;
using System.Collections.Generic;
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
using Gum.Mvvm;
using log4net;

namespace Bastion.Client.Screens.Matches;

public sealed class MatchAbandonmentViewModel : ViewModel, IDisposable
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(MatchAbandonmentViewModel));

    private readonly JoinedMatch _joinedMatch;
    private readonly IMatchClient _matches;
    private readonly SynchronizationContext? _uiContext;
    private readonly List<PlayerLeftEventArgs> _departures;

    public MatchAbandonmentViewModel(JoinedMatch joinedMatch, IMatchClient matches)
    {
        ArgumentNullException.ThrowIfNull(joinedMatch);
        ArgumentNullException.ThrowIfNull(matches);

        _joinedMatch = joinedMatch;
        _matches = matches;
        _uiContext = SynchronizationContext.Current;
        _departures = [];
        StatusText = string.Empty;
        PlayerLeftText = string.Empty;
        ConsequenceText = string.Empty;
        PreviewText = string.Empty;
        _matches.ParticipationEnded += HandleParticipationEnded;
    }

    private bool IsAbandoning
    {
        get => Get<bool>();
        set => Set(value);
    }

    [DependsOn(nameof(IsAbandoning))]
    public bool CanAbandon => !IsAbandoning;

    public bool IsConfirming
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public string ConsequenceText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public string PreviewText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public AbandonmentPreviewTone PreviewTone
    {
        get => Get<AbandonmentPreviewTone>();
        private set => Set(value);
    }

    [DependsOn(nameof(PreviewTone))]
    public bool IsPreviewWaiting => PreviewTone == AbandonmentPreviewTone.Waiting;

    [DependsOn(nameof(PreviewTone))]
    public bool IsPreviewLoss => PreviewTone == AbandonmentPreviewTone.Loss;

    [DependsOn(nameof(PreviewTone))]
    public bool IsPreviewInformation => PreviewTone == AbandonmentPreviewTone.Information;

    public string StatusText
    {
        get => Get<string>();
        private set => Set(value);
    }

    [DependsOn(nameof(StatusText))]
    public bool HasStatus => StatusText.Length > 0;

    public string PlayerLeftText
    {
        get => Get<string>();
        private set => Set(value);
    }

    [DependsOn(nameof(PlayerLeftText))]
    public bool HasPlayerLeft => PlayerLeftText.Length > 0;

    public IReadOnlyList<PlayerLeftEventArgs> Departures => _departures;

    public event EventHandler? ReturnToMenuRequested;

    public event EventHandler<PlayerLeftEventArgs>? PlayerLeft;

    public async Task OpenConfirmation()
    {
        if (IsConfirming || IsAbandoning)
        {
            return;
        }

        StatusText = string.Empty;
        ConsequenceText = AbandonmentText.DescribePendingConsequence(_joinedMatch);
        ShowPreviewNotice(Strings.MatchAbandonEloCalculating, AbandonmentPreviewTone.Waiting);
        IsConfirming = true;
        await LoadPreview();
    }

    public void CancelConfirmation()
    {
        IsConfirming = false;
    }

    public async Task ConfirmAbandonment()
    {
        IsConfirming = false;
        await AbandonMatch();
    }

    public async Task AbandonMatch()
    {
        if (IsAbandoning)
        {
            return;
        }

        IsAbandoning = true;
        StatusText = string.Empty;
        try
        {
            await LeaveJoinedMatch();
        }
        finally
        {
            IsAbandoning = false;
        }
    }

    public void Dispose()
    {
        _matches.ParticipationEnded -= HandleParticipationEnded;
    }

    private void HandleParticipationEnded(object? sender, ParticipationEndedEventArgs e)
    {
        if (!_joinedMatch.IsJoinedMatch(e.MatchId))
        {
            return;
        }

        Action receiveDeparture = () => ReceiveDeparture(e.DepartingTurnOrder, e.EndReason);
        UiThread.Run(_uiContext, receiveDeparture);
    }

    private void ReceiveDeparture(byte departingTurnOrder, EndedMatchReason endReason)
    {
        if (departingTurnOrder == _joinedMatch.GetOwnTurnOrder())
        {
            return;
        }

        string nickname = FindNickname(departingTurnOrder);
        var departure = new PlayerLeftEventArgs(nickname, endReason);
        _departures.Add(departure);
        string departureFormat = endReason == EndedMatchReason.Goal
            ? Strings.MatchPlayerReachedGoalFormat
            : Strings.MatchPlayerLeftFormat;
        PlayerLeftText = string.Format(CultureInfo.CurrentCulture, departureFormat, nickname);
        OnPlayerLeft(departure);
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

    private void OnPlayerLeft(PlayerLeftEventArgs e)
    {
        PlayerLeft?.Invoke(this, e);
    }

    private void ShowPreviewNotice(string text, AbandonmentPreviewTone tone)
    {
        PreviewText = text;
        PreviewTone = tone;
    }

    private async Task LoadPreview()
    {
        try
        {
            PreviewedAbandonment preview = await Task.Run(PreviewJoinedMatch);
            ShowPreview(preview);
        }
        catch (MatchNotJoinedException ex)
        {
            ReportPreviewFailure(ex);
        }
        catch (InvalidSessionException ex)
        {
            ReportPreviewFailure(ex);
        }
        catch (ServerUnreachableException ex)
        {
            ReportPreviewFailure(ex);
        }
        catch (ConnectionLostException ex)
        {
            ReportPreviewFailure(ex);
        }
        catch (ServerErrorException ex)
        {
            ReportPreviewFailure(ex);
        }
    }

    private PreviewedAbandonment PreviewJoinedMatch()
    {
        return _matches.PreviewAbandonment(_joinedMatch.Session.Token, _joinedMatch.FoundMatch.MatchId);
    }

    private void ShowPreview(PreviewedAbandonment preview)
    {
        ConsequenceText = AbandonmentText.DescribeConsequence(_joinedMatch, preview);
        if (!preview.IsRanked)
        {
            ShowPreviewNotice(Strings.MatchEndPrivateNotice, AbandonmentPreviewTone.Information);
            return;
        }

        if (preview.InitialElo is not short initialElo)
        {
            ShowPreviewNotice(Strings.MatchAbandonEloUnknown, AbandonmentPreviewTone.Loss);
            return;
        }

        if (preview.FinalElo is not short finalElo)
        {
            ShowPreviewNotice(Strings.MatchAbandonEloUnknown, AbandonmentPreviewTone.Loss);
            return;
        }

        string eloLossText = AbandonmentText.FormatEloLoss(initialElo, finalElo);
        ShowPreviewNotice(eloLossText, AbandonmentPreviewTone.Loss);
    }

    private void ReportPreviewFailure(Exception cause)
    {
        _logger.Warn("The cost of leaving the match could not be worked out.", cause);
        ShowPreviewNotice(Strings.MatchAbandonEloUnknown, AbandonmentPreviewTone.Loss);
    }

    private async Task LeaveJoinedMatch()
    {
        try
        {
            await Task.Run(AbandonJoinedMatch);
        }
        catch (MatchNotJoinedException ex)
        {
            _logger.Warn("The server says the player is not in that match.", ex);
            OnReturnToMenuRequested();
        }
        catch (InvalidSessionException ex)
        {
            _logger.Warn("The player could not leave the match because the session is no longer valid.", ex);
            StatusText = Strings.MatchAbandonFailed;
        }
        catch (ServerUnreachableException ex)
        {
            ReportAbandonFailure(ex);
        }
        catch (ConnectionLostException ex)
        {
            ReportAbandonFailure(ex);
        }
        catch (ServerErrorException ex)
        {
            ReportAbandonFailure(ex);
        }
    }

    private void AbandonJoinedMatch()
    {
        _matches.AbandonMatch(_joinedMatch.Session.Token, _joinedMatch.FoundMatch.MatchId);
    }

    private void ReportAbandonFailure(Exception cause)
    {
        _logger.Error("The player could not leave the match.", cause);
        StatusText = Strings.MatchAbandonFailed;
    }

    private void OnReturnToMenuRequested()
    {
        ReturnToMenuRequested?.Invoke(this, EventArgs.Empty);
    }
}
