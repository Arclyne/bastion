using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Matches;
using Bastion.Client.Screens.Matches;
using Bastion.Client.Tests.Fakes;
using Bastion.Domain.Matches;
using Xunit;

namespace Bastion.Client.Tests.Screens.Matches;

public class TestMatchAbandonmentViewModel : IDisposable
{
    private const int OtherMatchId = 40;

    private readonly FakeMatchClient _matchClient = new FakeMatchClient();
    private readonly MatchAbandonmentViewModel _viewModel;
    private int _returnToMenuRequestCount;

    public TestMatchAbandonmentViewModel()
    {
        JoinedMatch joinedMatch = JoinedMatchSamples.CreateWithOwnTurnOrder(Match.FirstTurnOrder);
        _viewModel = InlineUiContext.CreateViewModel(() => new MatchAbandonmentViewModel(joinedMatch, _matchClient));
        _viewModel.ReturnToMenuRequested += HandleReturnToMenuRequested;
    }

    public void Dispose()
    {
        _viewModel.Dispose();
    }

    [Fact]
    public async Task AbandonMatch_Accepted_AbandonsJoinedMatch()
    {
        await _viewModel.AbandonMatch();

        Assert.Equal(JoinedMatchSamples.MatchId, _matchClient.LastAbandonedMatchId);
    }

    [Fact]
    public async Task AbandonMatch_Accepted_RequestsReturnToMenu()
    {
        await _viewModel.AbandonMatch();

        Assert.Equal(1, _returnToMenuRequestCount);
    }

    [Fact]
    public async Task AbandonMatch_MatchAlreadyOver_RequestsReturnToMenu()
    {
        _matchClient.AbandonFailure = new MatchNotJoinedException();

        await _viewModel.AbandonMatch();

        Assert.Equal(1, _returnToMenuRequestCount);
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
        _matchClient.AbandonFailure = new ServerUnreachableException();

        await _viewModel.AbandonMatch();

        Assert.Equal(0, _returnToMenuRequestCount);
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
            JoinedMatchSamples.RivalNickname, true);

        _matchClient.RaiseMatchAbandoned(JoinedMatchSamples.MatchId, JoinedMatchSamples.RivalNickname, true);

        Assert.Equal(expectedText, _viewModel.RivalAbandonedText);
    }

    [Fact]
    public void MatchAbandoned_OtherMatch_IgnoresNotice()
    {
        _matchClient.RaiseMatchAbandoned(OtherMatchId, JoinedMatchSamples.RivalNickname, true);

        Assert.False(_viewModel.HasRivalAbandoned);
    }

    [Fact]
    public void MatchAbandoned_MatchGoesOn_ShowsPlayerLeftNotice()
    {
        string expectedText = string.Format(
            CultureInfo.CurrentCulture,
            Strings.MatchPlayerLeftFormat,
            JoinedMatchSamples.RivalNickname);

        _matchClient.RaiseMatchAbandoned(JoinedMatchSamples.MatchId, JoinedMatchSamples.RivalNickname, false);

        Assert.Equal(expectedText, _viewModel.PlayerLeftText);
    }

    [Fact]
    public void MatchAbandoned_MatchGoesOn_KeepsMatchOpen()
    {
        _matchClient.RaiseMatchAbandoned(JoinedMatchSamples.MatchId, JoinedMatchSamples.RivalNickname, false);

        Assert.False(_viewModel.HasRivalAbandoned);
    }

    [Fact]
    public void MatchAbandoned_MatchGoesOn_RemembersLeftPlayer()
    {
        _matchClient.RaiseMatchAbandoned(JoinedMatchSamples.MatchId, JoinedMatchSamples.RivalNickname, false);

        Assert.Equal(JoinedMatchSamples.RivalNickname, _viewModel.LeftNicknames.Single());
    }

    [Fact]
    public void RequestReturnToMenu_RivalAbandoned_RequestsReturnToMenu()
    {
        _matchClient.RaiseMatchAbandoned(JoinedMatchSamples.MatchId, JoinedMatchSamples.RivalNickname, true);

        _viewModel.RequestReturnToMenu();

        Assert.Equal(1, _returnToMenuRequestCount);
    }

    [Fact]
    public void Dispose_Subscribed_RemovesMatchAbandonedHandler()
    {
        _viewModel.Dispose();

        Assert.False(_matchClient.HasMatchAbandonedSubscribers());
    }

    private void HandleReturnToMenuRequested(object? sender, EventArgs e)
    {
        _returnToMenuRequestCount++;
    }
}
