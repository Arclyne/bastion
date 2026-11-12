using System;
using System.Globalization;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Chat;
using Bastion.Client.Screens.MainMenu.Chat;
using Bastion.Client.Screens.Matches;
using Bastion.Client.Tests.Fakes;
using Bastion.Domain.Chat;
using Bastion.Domain.Matches;
using Xunit;

namespace Bastion.Client.Tests.Screens.Matches;

public sealed class TestMatchChatViewModel : IDisposable
{
    private const string MessageText = "Good luck";
    private const string BlankText = "   ";
    private const string RivalMessageText = "You too";
    private const string TooFastMessage = "The player is sending messages too fast.";
    private const int OtherMatchId = 40;
    private const int SecondMessageNumber = 2;
    private const int NoMessagesSent = 0;

    private static readonly DateTimeOffset _openedAt = new DateTimeOffset(2026, 11, 5, 18, 1, 0, TimeSpan.Zero);
    private static readonly DateTime _rivalSentAt = new DateTime(2026, 11, 5, 18, 1, 30, DateTimeKind.Utc);

    private readonly FakeChatClient _chatClient = new FakeChatClient();
    private readonly ManualTimeProvider _timeProvider = new ManualTimeProvider(_openedAt);
    private readonly MatchChatViewModel _viewModel;

    public TestMatchChatViewModel()
    {
        JoinedMatch joinedMatch = JoinedMatchSampleFactory.CreateWithOwnTurnOrder(Match.FirstTurnOrder);
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

        Assert.Equal(JoinedMatchSampleFactory.MatchId, _chatClient.LastSentMatchId);
    }

    [Fact]
    public async Task SendMessage_ValidDraft_ConfirmsOwnRow()
    {
        _viewModel.Draft = MessageText;

        await _viewModel.SendMessage();

        ChatMessageRowViewModel ownRow = _viewModel.Messages[0];
        Assert.False(ownRow.IsPending);
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
        string expectedStatus = ChatRejectionText.DescribeInvalidMessage();

        await _viewModel.SendMessage();

        Assert.Equal(expectedStatus, _viewModel.StatusText);
    }

    [Fact]
    public async Task SendMessage_BlankDraft_SendsNothing()
    {
        _viewModel.Draft = BlankText;

        await _viewModel.SendMessage();

        Assert.Equal(NoMessagesSent, _chatClient.MatchChatSendCount);
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
        var cause = new InvalidOperationException(TooFastMessage);
        _chatClient.MatchChatSendFailure = new ChatMessageRejectedException(ChatRejection.TooFast, mutedUntil: null, cause);
        _viewModel.Draft = MessageText;
        string expectedState = ChatRejectionText.Describe(ChatRejection.TooFast);

        await _viewModel.SendMessage();

        ChatMessageRowViewModel ownRow = _viewModel.Messages[0];
        Assert.Equal(expectedState, ownRow.StateText);
    }

    [Fact]
    public async Task SendMessage_ServerUnreachable_ShowsChatUnavailableOnRow()
    {
        _chatClient.MatchChatSendFailure = new ServerUnreachableException();
        _viewModel.Draft = MessageText;

        await _viewModel.SendMessage();

        ChatMessageRowViewModel ownRow = _viewModel.Messages[0];
        Assert.Equal(Strings.MatchChatUnavailable, ownRow.StateText);
    }

    [Fact]
    public void MatchChatMessageReceived_JoinedMatch_AddsRivalRow()
    {
        ChatMessage rivalMessage = CreateRivalMessage(RivalMessageText);

        _chatClient.RaiseMatchChatMessageReceived(JoinedMatchSampleFactory.MatchId, rivalMessage);

        Assert.Single(_viewModel.Messages);
    }

    [Fact]
    public void MatchChatMessageReceived_OtherMatch_IgnoresMessage()
    {
        ChatMessage rivalMessage = CreateRivalMessage(RivalMessageText);

        _chatClient.RaiseMatchChatMessageReceived(OtherMatchId, rivalMessage);

        Assert.Empty(_viewModel.Messages);
    }

    [Fact]
    public void MatchChatMessageReceived_OverMaximumRecentMessages_DropsOldestMessage()
    {
        int messageCount = ChatPolicy.MaximumRecentMessages + 1;
        string expectedOldestText = SecondMessageNumber.ToString(CultureInfo.InvariantCulture);

        for (int messageNumber = 1; messageNumber <= messageCount; messageNumber++)
        {
            string text = messageNumber.ToString(CultureInfo.InvariantCulture);
            ChatMessage rivalMessage = CreateRivalMessage(text);
            _chatClient.RaiseMatchChatMessageReceived(JoinedMatchSampleFactory.MatchId, rivalMessage);
        }

        ChatMessageRowViewModel oldestRow = _viewModel.Messages[0];
        Assert.Equal(expectedOldestText, oldestRow.Text);
    }

    [Fact]
    public void Dispose_Subscribed_RemovesMessageHandler()
    {
        _viewModel.Dispose();

        bool hasSubscribers = _chatClient.HasMatchChatSubscribers();
        Assert.False(hasSubscribers);
    }

    private static ChatMessage CreateRivalMessage(string text)
    {
        var rival = new ChatAuthor(JoinedMatchSampleFactory.RivalNickname, isOwn: false);
        return new ChatMessage(rival, text, _rivalSentAt);
    }
}
