using System;
using System.Globalization;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Matches;
using Bastion.Client.Screens.Matches;
using Bastion.Client.Tests.Fakes;
using Bastion.Domain.Matches;
using Xunit;

namespace Bastion.Client.Tests.Screens.Matches;

public sealed class TestMatchAbandonmentViewModel : IDisposable
{
    private const int OtherMatchId = 40;
    private const int OneReturnRequest = 1;
    private const int NoReturnRequests = 0;

    private readonly FakeMatchClient _matchClient = new FakeMatchClient();
    private readonly MatchAbandonmentViewModel _viewModel;
    private int _returnToMenuRequestCount;

    public TestMatchAbandonmentViewModel()
    {
        JoinedMatch joinedMatch = JoinedMatchSampleFactory.CreateWithOwnTurnOrder(Match.FirstTurnOrder);
        _viewModel = InlineUiContext.CreateViewModel(() => new MatchAbandonmentViewModel(joinedMatch, _matchClient));
    }

    public void Dispose()
    {
        _viewModel.Dispose();
    }

    [Fact]
    public async Task AbandonMatch_Accepted_AbandonsJoinedMatch()
    {
        await _viewModel.AbandonMatch();

        Assert.Equal(JoinedMatchSampleFactory.MatchId, _matchClient.LastAbandonedMatchId);
    }

    [Fact]
    public async Task AbandonMatch_Accepted_RequestsReturnToMenu()
    {
        _viewModel.ReturnToMenuRequested += HandleReturnToMenuRequested;

        await _viewModel.AbandonMatch();

        Assert.Equal(OneReturnRequest, _returnToMenuRequestCount);
    }

    [Fact]
    public async Task AbandonMatch_MatchAlreadyOver_RequestsReturnToMenu()
    {
        _viewModel.ReturnToMenuRequested += HandleReturnToMenuRequested;
        _matchClient.AbandonFailure = new MatchNotJoinedException();

        await _viewModel.AbandonMatch();

        Assert.Equal(OneReturnRequest, _returnToMenuRequestCount);
    }

    [Fact]
    public async Task AbandonMatch_ServerUnreachable_ShowsAbandonFailed()
    {
        _matchClient.AbandonFailure = new ServerUnreachableException();

        await _viewModel.AbandonMatch();

        Assert.Equal(Strings.MatchAbandonFailed, _viewModel.StatusText);
    }

    [Fact]
    public async Task AbandonMatch_ServerUnreachable_StaysInMatch()
    {
        _viewModel.ReturnToMenuRequested += HandleReturnToMenuRequested;
        _matchClient.AbandonFailure = new ServerUnreachableException();

        await _viewModel.AbandonMatch();

        Assert.Equal(NoReturnRequests, _returnToMenuRequestCount);
    }

    [Fact]
    public async Task AbandonMatch_ServerUnreachable_AllowsAnotherTry()
    {
        _matchClient.AbandonFailure = new ServerUnreachableException();

        await _viewModel.AbandonMatch();

        Assert.True(_viewModel.CanAbandon);
    }

    [Fact]
    public void MatchAbandoned_JoinedMatch_ShowsRivalAbandoned()
    {
        string expectedText = string.Format(
            CultureInfo.CurrentCulture,
            Strings.MatchRivalAbandonedFormat,
            JoinedMatchSampleFactory.RivalNickname);

        _matchClient.RaiseMatchAbandoned(
            JoinedMatchSampleFactory.MatchId,
            JoinedMatchSampleFactory.RivalNickname,
            isMatchOver: true);

        Assert.Equal(expectedText, _viewModel.RivalAbandonedText);
    }

    [Fact]
    public void MatchAbandoned_OtherMatch_IgnoresNotice()
    {
        _matchClient.RaiseMatchAbandoned(OtherMatchId, JoinedMatchSampleFactory.RivalNickname, isMatchOver: true);

        Assert.False(_viewModel.HasRivalAbandoned);
    }

    [Fact]
    public void MatchAbandoned_MatchGoesOn_ShowsPlayerLeftNotice()
    {
        string expectedText = string.Format(
            CultureInfo.CurrentCulture,
            Strings.MatchPlayerLeftFormat,
            JoinedMatchSampleFactory.RivalNickname);

        _matchClient.RaiseMatchAbandoned(
            JoinedMatchSampleFactory.MatchId,
            JoinedMatchSampleFactory.RivalNickname,
            isMatchOver: false);

        Assert.Equal(expectedText, _viewModel.PlayerLeftText);
    }

    [Fact]
    public void MatchAbandoned_MatchGoesOn_KeepsMatchOpen()
    {
        _matchClient.RaiseMatchAbandoned(
            JoinedMatchSampleFactory.MatchId,
            JoinedMatchSampleFactory.RivalNickname,
            isMatchOver: false);

        Assert.False(_viewModel.HasRivalAbandoned);
    }

    [Fact]
    public void MatchAbandoned_MatchGoesOn_RemembersLeftPlayer()
    {
        string[] expectedLeftNicknames = { JoinedMatchSampleFactory.RivalNickname };

        _matchClient.RaiseMatchAbandoned(
            JoinedMatchSampleFactory.MatchId,
            JoinedMatchSampleFactory.RivalNickname,
            isMatchOver: false);

        Assert.Equal(expectedLeftNicknames, _viewModel.LeftNicknames);
    }

    [Fact]
    public void RequestReturnToMenu_RivalAbandoned_RequestsReturnToMenu()
    {
        _viewModel.ReturnToMenuRequested += HandleReturnToMenuRequested;
        _matchClient.RaiseMatchAbandoned(
            JoinedMatchSampleFactory.MatchId,
            JoinedMatchSampleFactory.RivalNickname,
            isMatchOver: true);

        _viewModel.RequestReturnToMenu();

        Assert.Equal(OneReturnRequest, _returnToMenuRequestCount);
    }

    [Fact]
    public void Dispose_Subscribed_RemovesMatchAbandonedHandler()
    {
        _viewModel.Dispose();

        bool hasSubscribers = _matchClient.HasMatchAbandonedSubscribers();
        Assert.False(hasSubscribers);
    }

    private void HandleReturnToMenuRequested(object? sender, EventArgs e)
    {
        _returnToMenuRequestCount++;
    }
}
