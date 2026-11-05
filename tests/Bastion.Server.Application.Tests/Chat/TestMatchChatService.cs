using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Chat;
using Bastion.Server.Application.Chat;
using Bastion.Server.Application.Matches;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Application.Tests.Fakes;
using Xunit;

namespace Bastion.Server.Application.Tests.Chat;

public class TestMatchChatService
{
    private const int MatchId = 16;
    private const int OtherMatchId = 12;
    private const int AuthorAccountId = 12;
    private const int RivalAccountId = 13;
    private const string AuthorNickname = "marta_muros";
    private const string AuthorToken = "author-token";
    private const string MessageText = "good luck";
    private const string PaddedMessageText = "  good luck  ";
    private const string StoreFailureMessage = "The database is not reachable.";

    private static readonly DateTime _now = new DateTime(2026, 11, 5, 20, 0, 0, DateTimeKind.Utc);

    private readonly FakeSessionVerifier _sessionVerifier = new FakeSessionVerifier();
    private readonly FakeMatchStore _matchStore = new FakeMatchStore();
    private readonly FakeMatchChatNotifier _matchChatNotifier = new FakeMatchChatNotifier();
    private readonly MatchChatService _matchChatService;

    public TestMatchChatService()
    {
        _matchChatService = new MatchChatService(_sessionVerifier, _matchStore, _matchChatNotifier);
        _sessionVerifier.Add(AuthorToken, new ConnectedPlayer(AuthorAccountId, AuthorNickname));
        _matchStore.UnfinishedParticipantAccountIdsByMatchId[MatchId] = new List<int>
        {
            AuthorAccountId,
            RivalAccountId,
        };
    }

    [Fact]
    public void SendMessage_Participant_SavesOneMessage()
    {
        _matchChatService.SendMessage(AuthorToken, new MatchChatSubmission(MatchId, MessageText), _now);

        Assert.Single(_matchStore.SavedChatMessages);
    }

    [Fact]
    public void SendMessage_Participant_NotifiesOnlyTheOtherParticipant()
    {
        _matchChatService.SendMessage(AuthorToken, new MatchChatSubmission(MatchId, MessageText), _now);

        Assert.Equal(RivalAccountId, _matchChatNotifier.NotifiedMessages.Single().RecipientAccountIds.Single());
    }

    [Fact]
    public void SendMessage_PaddedText_DeliversTrimmedText()
    {
        _matchChatService.SendMessage(AuthorToken, new MatchChatSubmission(MatchId, PaddedMessageText), _now);

        Assert.Equal(MessageText, _matchChatNotifier.NotifiedMessages.Single().Line.Text);
    }

    [Fact]
    public void SendMessage_PlayerOutsideMatch_ThrowsChatChannelForbidden()
    {
        var submission = new MatchChatSubmission(OtherMatchId, MessageText);

        Assert.Throws<ChatChannelForbiddenException>(
            () => _matchChatService.SendMessage(AuthorToken, submission, _now));
    }

    [Fact]
    public void SendMessage_BlankText_ThrowsInvalidChatMessage()
    {
        var submission = new MatchChatSubmission(MatchId, string.Empty);

        Assert.Throws<InvalidChatMessageException>(() => _matchChatService.SendMessage(AuthorToken, submission, _now));
    }

    [Fact]
    public void SendMessage_StoreFails_NotifiesNobody()
    {
        _matchStore.ChatMessageSaveFailure =
            new MatchStoreException(new InvalidOperationException(StoreFailureMessage));
        var submission = new MatchChatSubmission(MatchId, MessageText);

        Record.Exception(() => _matchChatService.SendMessage(AuthorToken, submission, _now));

        Assert.Empty(_matchChatNotifier.NotifiedMessages);
    }
}
