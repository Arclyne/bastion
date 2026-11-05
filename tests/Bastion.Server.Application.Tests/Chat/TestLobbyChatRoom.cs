using System;
using System.Linq;
using Bastion.Domain.Chat;
using Bastion.Server.Application.Chat;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Application.Tests.Fakes;
using Xunit;

namespace Bastion.Server.Application.Tests.Chat;

public class TestLobbyChatRoom
{
    private const int AccountId = 4;

    private static readonly DateTime _now = new DateTime(2026, 11, 5, 12, 0, 0, DateTimeKind.Utc);
    private static readonly ConnectedPlayer _sender = new ConnectedPlayer(AccountId, "marta_muros");

    private readonly FakeChatBroadcaster _broadcaster = new FakeChatBroadcaster();
    private readonly LobbyChatRoom _chatRoom;

    public TestLobbyChatRoom()
    {
        _chatRoom = new LobbyChatRoom(_broadcaster);
    }

    [Fact]
    public void Post_TwoLines_AssignsIncreasingIds()
    {
        PostedChatLine first = _chatRoom.Post(new ChatLine(_sender, "uno", _now));

        PostedChatLine second = _chatRoom.Post(new ChatLine(_sender, "dos", _now));

        Assert.True(second.MessageId > first.MessageId);
    }

    [Fact]
    public void Post_AboveCapacity_DropsOldestLine()
    {
        PostedChatLine oldest = _chatRoom.Post(new ChatLine(_sender, "viejo", _now));
        for (int lineIndex = 0; lineIndex < ChatRules.HistoryCapacity; lineIndex++)
        {
            _chatRoom.Post(new ChatLine(_sender, "nuevo", _now));
        }

        bool keepsOldest = _chatRoom.ListRecent(ChatRules.HistoryCapacity).Contains(oldest);

        Assert.False(keepsOldest);
    }

    [Fact]
    public void TryClaimTurn_FirstTurn_ReturnsTrue()
    {
        bool claimed = _chatRoom.TryClaimTurn(AccountId, _now);

        Assert.True(claimed);
    }

    [Fact]
    public void TryClaimTurn_JustBeforeCooldownEnds_ReturnsFalse()
    {
        _chatRoom.TryClaimTurn(AccountId, _now);

        bool claimed = _chatRoom.TryClaimTurn(AccountId, _now.AddSeconds(ChatRules.CooldownSeconds).AddTicks(-1));

        Assert.False(claimed);
    }
}
