using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Chat;
using Bastion.Domain.Moderation;
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
    private const string BannedTerm = "cheater";
    private const string BannedMessageText = "you cheater";
    private const string StoreFailureMessage = "The database is not reachable.";
    private const int SanctionDays = 1;

    private static readonly DateTime _now = new DateTime(2026, 11, 5, 20, 0, 0, DateTimeKind.Utc);

    private readonly FakeSessionVerifier _sessionVerifier = new FakeSessionVerifier();
    private readonly FakeUnitOfWork _unitOfWork = new FakeUnitOfWork();
    private readonly FakeMatchStore _matchStore = new FakeMatchStore();
    private readonly FakeMatchChatNotifier _matchChatNotifier = new FakeMatchChatNotifier();
    private readonly MatchChatService _matchChatService;

    public TestMatchChatService()
    {
        var matchChatRelay = new MatchChatRelay(_matchStore, _matchChatNotifier);
        _matchChatService = new MatchChatService(_sessionVerifier, new ChatModerator(_unitOfWork), matchChatRelay);
        _sessionVerifier.Add(AuthorToken, new ConnectedPlayer(AuthorAccountId, AuthorNickname));
        _unitOfWork.BannedWordStore.ChatTerms.Add(BannedTerm);
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

        Assert.Single(_matchStore.SavedMatchChatMessages);
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

        Assert.Equal(MessageText, _matchChatNotifier.NotifiedMessages.Single().PostedLine.Line.Text);
    }

    [Fact]
    public void SendMessage_PlayerOutsideMatch_ThrowsChatChannelForbidden()
    {
        var submission = new MatchChatSubmission(OtherMatchId, MessageText);

        Assert.Throws<ChatChannelForbiddenException>(
            () => _matchChatService.SendMessage(AuthorToken, submission, _now));
    }

    [Fact]
    public void SendMessage_BlankText_ThrowsChatMessageRejected()
    {
        var submission = new MatchChatSubmission(MatchId, string.Empty);

        Assert.Throws<ChatMessageRejectedException>(
            () => _matchChatService.SendMessage(AuthorToken, submission, _now));
    }

    [Fact]
    public void SendMessage_BannedTerm_NotifiesNobody()
    {
        var submission = new MatchChatSubmission(MatchId, BannedMessageText);

        Record.Exception(() => _matchChatService.SendMessage(AuthorToken, submission, _now));

        Assert.Empty(_matchChatNotifier.NotifiedMessages);
    }

    [Fact]
    public void SendMessage_ChatSanction_NotifiesNobody()
    {
        _unitOfWork.SanctionStore.Store(new Sanction(AuthorAccountId, SanctionScope.Chat, _now.AddDays(SanctionDays)));
        var submission = new MatchChatSubmission(MatchId, MessageText);

        Record.Exception(() => _matchChatService.SendMessage(AuthorToken, submission, _now));

        Assert.Empty(_matchChatNotifier.NotifiedMessages);
    }

    [Fact]
    public void SendMessage_SecondMessageWithinCooldown_NotifiesOnce()
    {
        var submission = new MatchChatSubmission(MatchId, MessageText);
        _matchChatService.SendMessage(AuthorToken, submission, _now);

        Record.Exception(() => _matchChatService.SendMessage(AuthorToken, submission, _now));

        Assert.Single(_matchChatNotifier.NotifiedMessages);
    }

    [Fact]
    public void SendMessage_StoreFails_NotifiesNobody()
    {
        _matchStore.MatchChatMessageSaveFailure =
            new MatchStoreException(new InvalidOperationException(StoreFailureMessage));
        var submission = new MatchChatSubmission(MatchId, MessageText);

        Record.Exception(() => _matchChatService.SendMessage(AuthorToken, submission, _now));

        Assert.Empty(_matchChatNotifier.NotifiedMessages);
    }
}
