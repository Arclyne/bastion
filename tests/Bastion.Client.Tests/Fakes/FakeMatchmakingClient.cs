using System;
using System.Collections.Generic;
using Bastion.Client.Networking.Matchmaking;
using Bastion.Client.Networking.Notifications;

namespace Bastion.Client.Tests.Fakes;

public sealed class FakeMatchmakingClient : IMatchmakingClient
{
    private const int FoundMatchId = 12;
    private const byte ClassicBoardSize = 9;
    private const byte ClassicWallsPerPlayer = 10;
    private const byte ClockMinutes = 5;

    private static readonly FoundMatchFormat _classicFormat =
        new FoundMatchFormat(ClassicBoardSize, ClassicWallsPerPlayer, ClockMinutes);

    public Exception? StartSearchFailure { get; set; }

    public IReadOnlyList<FoundMatchParticipant>? MatchFoundDuringStart { get; set; }

    public int CancelSearchCount { get; private set; }

    public int ConfirmPresenceCount { get; private set; }

    public event EventHandler<MatchFoundEventArgs>? MatchFound;

    public event EventHandler<PresenceRequestedEventArgs>? PresenceRequested;

    public event EventHandler<SearchEndedEventArgs>? SearchEnded;

    public event EventHandler? Disconnected;

    public void StartSearch(string sessionToken)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        if (StartSearchFailure is not null)
        {
            throw StartSearchFailure;
        }

        if (MatchFoundDuringStart is not null)
        {
            RaiseMatchFound(MatchFoundDuringStart);
        }
    }

    public void CancelSearch(string sessionToken)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        CancelSearchCount++;
    }

    public void ConfirmPresence(string sessionToken)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        ConfirmPresenceCount++;
    }

    public bool HasSubscribers()
    {
        return MatchFound is not null
            || PresenceRequested is not null
            || SearchEnded is not null
            || Disconnected is not null;
    }

    public void RaiseMatchFound(IReadOnlyList<FoundMatchParticipant> participants)
    {
        ArgumentNullException.ThrowIfNull(participants);

        var matchFound = new MatchFoundEventArgs(FoundMatchId, _classicFormat, participants);
        MatchFound?.Invoke(this, matchFound);
    }

    public void RaisePresenceRequested(TimeSpan responseWindow)
    {
        var presenceRequest = new PresenceRequestedEventArgs(responseWindow);
        PresenceRequested?.Invoke(this, presenceRequest);
    }

    public void RaiseSearchEnded(SearchEndReason reason)
    {
        var searchEnd = new SearchEndedEventArgs(reason);
        SearchEnded?.Invoke(this, searchEnd);
    }

    public void RaiseDisconnected()
    {
        Disconnected?.Invoke(this, EventArgs.Empty);
    }
}
