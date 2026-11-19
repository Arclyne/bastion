using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Matches;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Screens.Matches;
using Bastion.Client.Tests.Fakes;
using Bastion.Domain.Matches;
using Xunit;

namespace Bastion.Client.Tests.Screens.Matches;

public sealed class TestMatchDrawViewModel : IDisposable
{
    private const int OtherMatchId = 40;
    private const int ThreeMovesLeft = 3;

    private static readonly DateTimeOffset _now = new DateTimeOffset(2026, 11, 19, 18, 0, 0, TimeSpan.Zero);

    private readonly FakeMatchClient _matchClient = new FakeMatchClient();
    private readonly FakeNotificationClient _notificationClient = new FakeNotificationClient();
    private readonly ManualTimeProvider _timeProvider = new ManualTimeProvider(_now);
    private readonly List<IDisposable> _viewModels = [];
    private readonly JoinedMatch _joinedMatch = JoinedMatchSampleFactory.CreateWithOwnTurnOrder(Match.FirstTurnOrder);
    private readonly MatchPlayViewModel _matchPlay;
    private readonly MatchDrawViewModel _draw;

    public TestMatchDrawViewModel()
    {
        Func<MatchPlayViewModel> createMatchPlay = () => new MatchPlayViewModel(
            _joinedMatch,
            _matchClient,
            _timeProvider);
        _matchPlay = InlineUiContext.CreateViewModel(createMatchPlay);
        var services = new MatchConnectionServices(_notificationClient, _matchClient, _timeProvider);
        Func<MatchDrawViewModel> createDraw = () => new MatchDrawViewModel(_joinedMatch, _matchPlay, services);
        _draw = InlineUiContext.CreateViewModel(createDraw);
        _viewModels.Add(_draw);
        _viewModels.Add(_matchPlay);
    }

    public void Dispose()
    {
        _viewModels.ForEach(viewModel => viewModel.Dispose());
    }

    [Fact]
    public void Constructor_TwoPlayers_ShowsTheOfferButton()
    {
        Assert.True(_draw.IsOfferButtonShown);
    }

    [Fact]
    public void Constructor_FourPlayers_HidesTheOfferButton()
    {
        JoinedMatch fourPlayerMatch = JoinedMatchSampleFactory.CreateFourPlayerWithOwnThirdTurn();
        var services = new MatchConnectionServices(_notificationClient, _matchClient, _timeProvider);
        Func<MatchPlayViewModel> createMatchPlay = () => new MatchPlayViewModel(
            fourPlayerMatch,
            _matchClient,
            _timeProvider);
        MatchPlayViewModel fourPlayerPlay = InlineUiContext.CreateViewModel(createMatchPlay);
        _viewModels.Add(fourPlayerPlay);
        Func<MatchDrawViewModel> createDraw = () => new MatchDrawViewModel(fourPlayerMatch, fourPlayerPlay, services);

        MatchDrawViewModel fourPlayerDraw = InlineUiContext.CreateViewModel(createDraw);

        _viewModels.Add(fourPlayerDraw);
        Assert.False(fourPlayerDraw.IsOfferButtonShown);
    }

    [Fact]
    public async Task OfferDraw_ServerUp_SendsTheOffer()
    {
        await _draw.OfferDraw();

        Assert.Equal(1, _matchClient.DrawOfferCount);
    }

    [Fact]
    public async Task OfferDraw_ServerUp_MarksTheOwnOfferPending()
    {
        await _draw.OfferDraw();

        Assert.True(_draw.IsOwnOfferPending);
    }

    [Fact]
    public async Task OfferDraw_OfferPending_CannotOfferAgain()
    {
        await _draw.OfferDraw();

        Assert.False(_draw.CanOfferDraw);
    }

    [Fact]
    public async Task OfferDraw_TooSoon_TakesTheMovesLeft()
    {
        var cause = new InvalidOperationException();
        _matchClient.DrawOfferFailure = new DrawOfferRejectedException(DrawOfferRefusal.TooSoon, ThreeMovesLeft, cause);

        await _draw.OfferDraw();

        Assert.Equal(ThreeMovesLeft, _draw.MovesLeft);
    }

    [Fact]
    public async Task OfferDraw_ServerDown_ShowsTheFailure()
    {
        _matchClient.DrawOfferFailure = new ServerUnreachableException();

        await _draw.OfferDraw();

        Assert.True(_draw.HasToast);
    }

    [Fact]
    public void DrawOffered_ByTheRival_ShowsTheOffer()
    {
        _matchClient.RaiseDrawOffered(CreateDraw(JoinedMatchSampleFactory.MatchId, Match.SecondTurnOrder));

        Assert.True(_draw.HasRivalOffer);
    }

    [Fact]
    public void DrawOffered_OtherMatch_ShowsNothing()
    {
        _matchClient.RaiseDrawOffered(CreateDraw(OtherMatchId, Match.SecondTurnOrder));

        Assert.False(_draw.HasRivalOffer);
    }

    [Fact]
    public async Task AcceptDraw_RivalOffer_SendsTheAcceptance()
    {
        _matchClient.RaiseDrawOffered(CreateDraw(JoinedMatchSampleFactory.MatchId, Match.SecondTurnOrder));

        await _draw.AcceptDraw();

        Assert.True(_matchClient.LastDrawAnswer);
    }

    [Fact]
    public async Task RejectDraw_RivalOffer_HidesTheOffer()
    {
        _matchClient.RaiseDrawOffered(CreateDraw(JoinedMatchSampleFactory.MatchId, Match.SecondTurnOrder));

        await _draw.RejectDraw();

        Assert.False(_draw.HasRivalOffer);
    }

    [Fact]
    public async Task RejectDraw_OfferExpiredMeanwhile_HidesTheOffer()
    {
        _matchClient.RaiseDrawOffered(CreateDraw(JoinedMatchSampleFactory.MatchId, Match.SecondTurnOrder));
        _matchClient.DrawAnswerFailure = new DrawNotPendingException();

        await _draw.RejectDraw();

        Assert.False(_draw.HasRivalOffer);
    }

    [Fact]
    public async Task DrawRejected_OwnOffer_WaitsFiveMoves()
    {
        await _draw.OfferDraw();

        _matchClient.RaiseDrawRejected(CreateDraw(JoinedMatchSampleFactory.MatchId, Match.FirstTurnOrder));

        Assert.Equal(MatchDrawPolicy.OwnMovesBetweenOffers, _draw.MovesLeft);
    }

    [Fact]
    public async Task DrawRejected_OwnOffer_SaysItWasDeclined()
    {
        await _draw.OfferDraw();

        _matchClient.RaiseDrawRejected(CreateDraw(JoinedMatchSampleFactory.MatchId, Match.FirstTurnOrder));

        Assert.True(_draw.HasToast);
    }

    [Fact]
    public void DrawOfferExpired_RivalOffer_HidesTheOffer()
    {
        _matchClient.RaiseDrawOffered(CreateDraw(JoinedMatchSampleFactory.MatchId, Match.SecondTurnOrder));

        _matchClient.RaiseDrawOfferExpired(CreateDraw(JoinedMatchSampleFactory.MatchId, Match.SecondTurnOrder));

        Assert.False(_draw.HasRivalOffer);
    }

    [Fact]
    public async Task PawnMoved_OwnMoveWhileWaiting_CountsDownTheMovesLeft()
    {
        await _draw.OfferDraw();
        _matchClient.RaiseDrawRejected(CreateDraw(JoinedMatchSampleFactory.MatchId, Match.FirstTurnOrder));
        PawnMovedEventArgs ownMove = PawnMovedSampleFactory.CreateFirstMove(
            JoinedMatchSampleFactory.MatchId,
            JoinedMatchSampleFactory.OwnNickname,
            "e2");

        _matchClient.RaisePawnMoved(ownMove);

        Assert.Equal(MatchDrawPolicy.OwnMovesBetweenOffers - 1, _draw.MovesLeft);
    }

    [Fact]
    public void MatchEnded_RivalOffer_HidesTheOffer()
    {
        _matchClient.RaiseDrawOffered(CreateDraw(JoinedMatchSampleFactory.MatchId, Match.SecondTurnOrder));
        MatchEndedEventArgs matchEnd = MatchEndSampleFactory.CreateRivalAbandonment(Match.FirstTurnOrder);

        _matchClient.RaiseMatchEnded(matchEnd);

        Assert.False(_draw.HasRivalOffer);
    }

    [Fact]
    public void ApplyRejoinedMatch_RivalOfferPending_ShowsTheOffer()
    {
        RejoinedMatch rejoinedMatch = RejoinedMatchSampleFactory.CreateAfterRivalWallAndMove(_joinedMatch) with
        {
            PendingDrawOffererTurnOrder = Match.SecondTurnOrder,
        };

        _matchPlay.ApplyRejoinedMatch(rejoinedMatch);

        Assert.True(_draw.HasRivalOffer);
    }

    private static DrawEventArgs CreateDraw(int matchId, byte offererTurnOrder)
    {
        return new DrawEventArgs
        {
            MatchId = matchId,
            OffererTurnOrder = offererTurnOrder,
        };
    }
}
