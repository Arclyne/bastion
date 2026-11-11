using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Matches;
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
    private readonly List<string> _leftNicknames = new List<string>();

    public MatchAbandonmentViewModel(JoinedMatch joinedMatch, IMatchClient matches)
    {
        ArgumentNullException.ThrowIfNull(joinedMatch);
        ArgumentNullException.ThrowIfNull(matches);

        _joinedMatch = joinedMatch;
        _matches = matches;
        _uiContext = SynchronizationContext.Current;
        StatusText = string.Empty;
        RivalAbandonedText = string.Empty;
        PlayerLeftText = string.Empty;
        _matches.MatchAbandoned += HandleMatchAbandoned;
    }

    public bool IsAbandoning
    {
        get => Get<bool>();
        private set => Set(value);
    }

    [DependsOn(nameof(IsAbandoning))]
    public bool CanAbandon => !IsAbandoning;

    public string StatusText
    {
        get => Get<string>();
        private set => Set(value);
    }

    [DependsOn(nameof(StatusText))]
    public bool HasStatus => StatusText.Length > 0;

    public string RivalAbandonedText
    {
        get => Get<string>();
        private set => Set(value);
    }

    [DependsOn(nameof(RivalAbandonedText))]
    public bool HasRivalAbandoned => RivalAbandonedText.Length > 0;

    public string PlayerLeftText
    {
        get => Get<string>();
        private set => Set(value);
    }

    [DependsOn(nameof(PlayerLeftText))]
    public bool HasPlayerLeft => PlayerLeftText.Length > 0;

    public IReadOnlyList<string> LeftNicknames => _leftNicknames;

    public event EventHandler? ReturnToMenuRequested;

    public event EventHandler<PlayerLeftEventArgs>? PlayerLeft;

    public async Task AbandonMatch()
    {
        if (IsAbandoning)
        {
            return;
        }

        IsAbandoning = true;
        StatusText = string.Empty;
        bool isMatchOver;
        try
        {
            isMatchOver = await TryAbandonJoinedMatch();
        }
        finally
        {
            IsAbandoning = false;
        }

        if (isMatchOver)
        {
            OnReturnToMenuRequested();
        }
    }

    public void RequestReturnToMenu()
    {
        OnReturnToMenuRequested();
    }

    public void Dispose()
    {
        _matches.MatchAbandoned -= HandleMatchAbandoned;
    }

    private async Task<bool> TryAbandonJoinedMatch()
    {
        try
        {
            await Task.Run(AbandonJoinedMatch);
            return true;
        }
        catch (MatchNotJoinedException ex)
        {
            _logger.Warn("The match was already closed when the player left it.", ex);
            return true;
        }
        catch (InvalidSessionException ex)
        {
            ReportAbandonFailure(ex);
            return false;
        }
        catch (ServerUnreachableException ex)
        {
            ReportAbandonFailure(ex);
            return false;
        }
        catch (ConnectionLostException ex)
        {
            ReportAbandonFailure(ex);
            return false;
        }
        catch (ServerErrorException ex)
        {
            ReportAbandonFailure(ex);
            return false;
        }
    }

    private void AbandonJoinedMatch()
    {
        _matches.AbandonMatch(_joinedMatch.Session.Token, _joinedMatch.FoundMatch.MatchId);
    }

    private void ReportAbandonFailure(Exception ex)
    {
        _logger.Warn("The player could not leave the match.", ex);
        StatusText = Strings.MatchAbandonFailed;
    }

    private void ShowRivalAbandoned(string abandoningNickname)
    {
        RivalAbandonedText = string.Format(
            CultureInfo.CurrentCulture,
            Strings.MatchRivalAbandonedFormat,
            abandoningNickname);
    }

    private void ReceiveAbandonment(string abandoningNickname, bool isMatchOver)
    {
        if (isMatchOver)
        {
            ShowRivalAbandoned(abandoningNickname);
            return;
        }

        _leftNicknames.Add(abandoningNickname);
        PlayerLeftText = string.Format(CultureInfo.CurrentCulture, Strings.MatchPlayerLeftFormat, abandoningNickname);
        OnPlayerLeft(new PlayerLeftEventArgs(abandoningNickname));
    }

    private void OnPlayerLeft(PlayerLeftEventArgs e)
    {
        PlayerLeft?.Invoke(this, e);
    }

    private void OnReturnToMenuRequested()
    {
        ReturnToMenuRequested?.Invoke(this, EventArgs.Empty);
    }

    private void HandleMatchAbandoned(object? sender, MatchAbandonedEventArgs e)
    {
        if (e.MatchId != _joinedMatch.FoundMatch.MatchId)
        {
            return;
        }

        UiThread.Run(_uiContext, () => ReceiveAbandonment(e.AbandoningNickname, e.IsMatchOver));
    }
}
