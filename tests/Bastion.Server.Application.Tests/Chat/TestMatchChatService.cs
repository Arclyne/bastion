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

public sealed class TestMatchChatService
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
    private const string RepositoryFailureMessage = "The database is not reachable.";
    private const int SanctionDays = 1;

    private static readonly DateTime _now = new DateTime(2026, 11, 5, 20, 0, 0, DateTimeKind.Utc);
    private static readonly ConnectedPlayer _author = new ConnectedPlayer(AuthorAccountId, AuthorNickname);
    private static readonly MatchChatSubmission _submission = new MatchChatSubmission(MatchId, MessageText);

    private readonly FakeUnitOfWork _unitOfWork = new FakeUnitOfWork();
    private readonly FakeMatchRepository _matchRepository = new FakeMatchRepository();
    private readonly FakeMatchChatNotifier _matchChatNotifier = new FakeMatchChatNotifier();
    private readonly MatchChatService _matchChatService;

    public TestMatchChatService()
    {
        var sessionVerifier = new FakeSessionVerifier();
        var moderator = new ChatModerator(_unitOfWork);
        var matchChatRelay = new MatchChatRelay(_matchRepository, _matchChatNotifier);
        _matchChatService = new MatchChatService(sessionVerifier, moderator, matchChatRelay);
        sessionVerifier.Add(AuthorToken, _author);
    }

    [Fact]
    public void SendMessage_Participant_SavesOneMessage()
    {
        AddParticipants();

        _matchChatService.SendMessage(AuthorToken, _submission, _now);

        Assert.Single(_matchRepository.SavedMatchChatMessages);
    }

    [Fact]
    public void SendMessage_Participant_NotifiesOnlyTheOtherParticipant()
    {
        AddParticipants();
        int[] expectedRecipients = { RivalAccountId };

        _matchChatService.SendMessage(AuthorToken, _submission, _now);

        NotifiedMatchChatMessage notified = _matchChatNotifier.NotifiedMessages.Single();
        Assert.Equal(expectedRecipients, notified.RecipientAccountIds);
    }

    [Fact]
    public void SendMessage_PaddedText_DeliversTrimmedText()
    {
        AddParticipants();
        var submission = new MatchChatSubmission(MatchId, PaddedMessageText);

        _matchChatService.SendMessage(AuthorToken, submission, _now);

        NotifiedMatchChatMessage notified = _matchChatNotifier.NotifiedMessages.Single();
        Assert.Equal(MessageText, notified.PostedLine.Line.Text);
    }

    [Theory]
    [InlineData("Good luck, have fun!")]
    [InlineData("¡Suerte! ¿Quién mueve el peón, Ñandú?")]
    public void SendMessage_EnglishOrSpanishText_DeliversSameText(string text)
    {
        AddParticipants();
        var submission = new MatchChatSubmission(MatchId, text);

        _matchChatService.SendMessage(AuthorToken, submission, _now);

        NotifiedMatchChatMessage notified = _matchChatNotifier.NotifiedMessages.Single();
        Assert.Equal(text, notified.PostedLine.Line.Text);
    }

    [Fact]
    public void SendMessage_PlayerOutsideMatch_ThrowsChatChannelForbiddenException()
    {
        var submission = new MatchChatSubmission(OtherMatchId, MessageText);

        Action send = () => _matchChatService.SendMessage(AuthorToken, submission, _now);

        Assert.Throws<ChatChannelForbiddenException>(send);
    }

    [Fact]
    public void SendMessage_EmptyText_RejectsInvalidMessage()
    {
        AddParticipants();
        var submission = new MatchChatSubmission(MatchId, string.Empty);
        Action send = () => _matchChatService.SendMessage(AuthorToken, submission, _now);

        Exception? exception = Record.Exception(send);

        var rejection = exception as ChatMessageRejectedException;
        Assert.Equal(ChatRejection.InvalidMessage, rejection?.Rejection);
    }

    [Fact]
    public void SendMessage_BannedTerm_NotifiesNobody()
    {
        AddParticipants();
        _unitOfWork.BannedWordRepository.ChatTerms.Add(BannedTerm);
        var submission = new MatchChatSubmission(MatchId, BannedMessageText);
        Action send = () => _matchChatService.SendMessage(AuthorToken, submission, _now);

        Record.Exception(send);

        Assert.Empty(_matchChatNotifier.NotifiedMessages);
    }

    [Fact]
    public void SendMessage_ChatSanction_NotifiesNobody()
    {
        AddParticipants();
        DateTime muteEnd = _now.AddDays(SanctionDays);
        var sanction = new Sanction(AuthorAccountId, SanctionScope.Chat, muteEnd);
        _unitOfWork.SanctionRepository.AddSanction(sanction);
        Action send = () => _matchChatService.SendMessage(AuthorToken, _submission, _now);

        Record.Exception(send);

        Assert.Empty(_matchChatNotifier.NotifiedMessages);
    }

    [Fact]
    public void SendMessage_SecondMessageWithinCooldown_NotifiesOnce()
    {
        AddParticipants();
        _matchChatService.SendMessage(AuthorToken, _submission, _now);
        Action sendAgain = () => _matchChatService.SendMessage(AuthorToken, _submission, _now);

        Record.Exception(sendAgain);

        Assert.Single(_matchChatNotifier.NotifiedMessages);
    }

    [Fact]
    public void SendMessage_RepositoryFails_NotifiesNobody()
    {
        AddParticipants();
        var cause = new InvalidOperationException(RepositoryFailureMessage);
        _matchRepository.MatchChatMessageSaveFailure = new MatchRepositoryException(cause);
        Action send = () => _matchChatService.SendMessage(AuthorToken, _submission, _now);

        Record.Exception(send);

        Assert.Empty(_matchChatNotifier.NotifiedMessages);
    }

    private void AddParticipants()
    {
        var participantAccountIds = new List<int>
        {
            AuthorAccountId,
            RivalAccountId,
        };
        _matchRepository.UnfinishedParticipantAccountIdsByMatchId[MatchId] = participantAccountIds;
    }
}
