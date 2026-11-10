using System;
using System.Globalization;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Chat;
using Bastion.Client.Screens.MainMenu.Chat;
using Bastion.Client.Screens.Matches;
using Bastion.Client.Tests.Fakes;
using Bastion.Domain.Chat;
using Bastion.Domain.Matches;
using Xunit;

namespace Bastion.Client.Tests.Screens.Matches;

public class TestMatchChatViewModel : IDisposable
{
    private const string MessageText = "Good luck";
    private const string BlankText = "   ";
    private const string RivalMessageText = "You too";
    private const string TooFastMessage = "The player is sending messages too fast.";
    private const int OtherMatchId = 40;
    private const int SecondMessageNumber = 2;

    private static readonly DateTimeOffset _openedAt = new DateTimeOffset(2026, 11, 5, 18, 1, 0, TimeSpan.Zero);
    private static readonly DateTime _rivalSentAt = new DateTime(2026, 11, 5, 18, 1, 30, DateTimeKind.Utc);

    private readonly FakeChatClient _chatClient = new FakeChatClient();
    private readonly ManualTimeProvider _timeProvider = new ManualTimeProvider(_openedAt);
    private readonly MatchChatViewModel _viewModel;

    public TestMatchChatViewModel()
    {
        JoinedMatch joinedMatch = JoinedMatchSamples.CreateWithOwnTurnOrder(Match.FirstTurnOrder);
        _viewModel = InlineUiContext.CreateViewModel(
            () => new MatchChatViewModel(joinedMatch, _chatClient, _timeProvider));
    }

    public void Dispose()
    {
        _viewModel.Dispose();
    }

    [Fact]
    public async Task SendMessage_ValidDraft_SendsToJoinedMatch()
    {
        _viewModel.Draft = MessageText;

        await _viewModel.SendMessage();

        Assert.Equal(JoinedMatchSamples.MatchId, _chatClient.LastSentMatchId);
    }

    [Fact]
    public async Task SendMessage_ValidDraft_ConfirmsOwnRow()
    {
        _viewModel.Draft = MessageText;

        await _viewModel.SendMessage();

        Assert.False(_viewModel.Messages[0].IsPending);
    }

    [Fact]
    public async Task SendMessage_ValidDraft_ClearsDraft()
    {
        _viewModel.Draft = MessageText;

        await _viewModel.SendMessage();

        Assert.Empty(_viewModel.Draft);
    }

    [Fact]
    public async Task SendMessage_BlankDraft_ShowsInvalidMessageStatus()
    {
        _viewModel.Draft = BlankText;

        await _viewModel.SendMessage();

        Assert.Equal(ChatRejectionText.DescribeInvalidMessage(), _viewModel.StatusText);
    }

    [Fact]
    public async Task SendMessage_BlankDraft_SendsNothing()
    {
        _viewModel.Draft = BlankText;

        await _viewModel.SendMessage();

        Assert.Equal(0, _chatClient.MatchChatSendCount);
    }

    [Fact]
    public async Task Draft_ChangedAfterStatus_ClearsStatus()
    {
        _viewModel.Draft = BlankText;
        await _viewModel.SendMessage();

        _viewModel.Draft = MessageText;

        Assert.False(_viewModel.HasStatus);
    }

    [Fact]
    public async Task SendMessage_TooFast_ShowsRejectionOnRow()
    {
        _chatClient.MatchChatSendFailure = new ChatMessageRejectedException(
            ChatRejection.TooFast,
            null,
            new InvalidOperationException(TooFastMessage));
        _viewModel.Draft = MessageText;

        await _viewModel.SendMessage();

        Assert.Equal(ChatRejectionText.Describe(ChatRejection.TooFast), _viewModel.Messages[0].StateText);
    }

    [Fact]
    public async Task SendMessage_ServerUnreachable_ShowsChatUnavailableOnRow()
    {
        _chatClient.MatchChatSendFailure = new ServerUnreachableException();
        _viewModel.Draft = MessageText;

        await _viewModel.SendMessage();

        Assert.Equal(Strings.MatchChatUnavailable, _viewModel.Messages[0].StateText);
    }

    [Fact]
    public void MatchChatMessageReceived_JoinedMatch_AddsRivalRow()
    {
        _chatClient.RaiseMatchChatMessageReceived(JoinedMatchSamples.MatchId, CreateRivalMessage(RivalMessageText));

        Assert.Single(_viewModel.Messages);
    }

    [Fact]
    public void MatchChatMessageReceived_OtherMatch_IgnoresMessage()
    {
        _chatClient.RaiseMatchChatMessageReceived(OtherMatchId, CreateRivalMessage(RivalMessageText));

        Assert.Empty(_viewModel.Messages);
    }

    [Fact]
    public void MatchChatMessageReceived_OverMaximumRecentMessages_DropsOldestMessage()
    {
        for (int messageNumber = 1; messageNumber <= ChatPolicy.MaximumRecentMessages + 1; messageNumber++)
        {
            string text = messageNumber.ToString(CultureInfo.InvariantCulture);
            _chatClient.RaiseMatchChatMessageReceived(JoinedMatchSamples.MatchId, CreateRivalMessage(text));
        }

        Assert.Equal(SecondMessageNumber.ToString(CultureInfo.InvariantCulture), _viewModel.Messages[0].Text);
    }

    [Fact]
    public void Dispose_Subscribed_RemovesMessageHandler()
    {
        _viewModel.Dispose();

        Assert.False(_chatClient.HasMatchChatSubscribers());
    }

    private static ChatMessage CreateRivalMessage(string text)
    {
        var rival = new ChatAuthor(JoinedMatchSamples.RivalNickname, false);
        return new ChatMessage(rival, text, _rivalSentAt);
    }
}
