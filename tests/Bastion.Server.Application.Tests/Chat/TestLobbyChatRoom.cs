using System;
using System.Collections.Generic;
using Bastion.Domain.Chat;
using Bastion.Server.Application.Chat;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Application.Tests.Fakes;
using Xunit;

namespace Bastion.Server.Application.Tests.Chat;

public sealed class TestLobbyChatRoom
{
    private const int AccountId = 4;
    private const string Nickname = "marta_muros";
    private const string FirstText = "uno";
    private const string SecondText = "dos";
    private const string OldestText = "viejo";
    private const string NewerText = "nuevo";

    private static readonly DateTime _now = new DateTime(2026, 11, 5, 12, 0, 0, DateTimeKind.Utc);
    private static readonly ConnectedPlayer _sender = new ConnectedPlayer(AccountId, Nickname);

    private readonly LobbyChatRoom _chatRoom;

    public TestLobbyChatRoom()
    {
        var broadcaster = new FakeChatBroadcaster();
        _chatRoom = new LobbyChatRoom(broadcaster);
    }

    [Fact]
    public void Post_TwoLines_AssignsIncreasingIds()
    {
        var firstLine = new ChatLine(_sender, FirstText, _now);
        var secondLine = new ChatLine(_sender, SecondText, _now);
        PostedChatLine first = _chatRoom.Post(firstLine);

        PostedChatLine second = _chatRoom.Post(secondLine);

        bool isIncreasing = second.MessageId > first.MessageId;
        Assert.True(isIncreasing);
    }

    [Fact]
    public void Post_AboveCapacity_DropsOldestLine()
    {
        var oldestLine = new ChatLine(_sender, OldestText, _now);
        var newerLine = new ChatLine(_sender, NewerText, _now);
        PostedChatLine oldest = _chatRoom.Post(oldestLine);

        for (int lineIndex = 0; lineIndex < ChatPolicy.MaximumRecentMessages; lineIndex++)
        {
            _chatRoom.Post(newerLine);
        }

        IReadOnlyList<PostedChatLine> recentLines = _chatRoom.ListRecent();
        Assert.DoesNotContain(oldest, recentLines);
    }
}
