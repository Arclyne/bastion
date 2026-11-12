using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Screens.Matches;
using Bastion.Domain.Matches;

namespace Bastion.Client.Tests.Screens.Matches;

public static class JoinedMatchSampleFactory
{
    public const int MatchId = 12;
    public const string OwnNickname = "marta_muros";
    public const string RivalNickname = "leo_peon";
    public const short RivalEloRating = 1000;
    public const byte BoardSize = 9;
    public const byte WallsPerPlayer = 10;
    public const string SecondRivalNickname = "luis_quo";
    public const string ThirdRivalNickname = "sofia_salto";

    private const short OwnEloRating = 1210;
    private const byte ClockMinutes = 5;
    private const byte FourPlayerWallsPerPlayer = 5;
    private const string SessionToken = "session-token";
    private const string PreferredLanguage = "es-MX";

    public static JoinedMatch CreateWithOwnTurnOrder(byte ownTurnOrder)
    {
        byte rivalTurnOrder = ownTurnOrder == Match.FirstTurnOrder ? Match.SecondTurnOrder : Match.FirstTurnOrder;
        var rival = new FoundMatchParticipant(RivalNickname, RivalEloRating, rivalTurnOrder);
        var own = new FoundMatchParticipant(OwnNickname, OwnEloRating, ownTurnOrder);
        FoundMatchParticipant[] participants = { rival, own };
        var format = new FoundMatchFormat(BoardSize, WallsPerPlayer, ClockMinutes);
        return Create(format, participants);
    }

    public static JoinedMatch CreateFourPlayerWithOwnThirdTurn()
    {
        var thirdRival = new FoundMatchParticipant(ThirdRivalNickname, RivalEloRating, Match.FourthTurnOrder);
        var rival = new FoundMatchParticipant(RivalNickname, RivalEloRating, Match.FirstTurnOrder);
        var own = new FoundMatchParticipant(OwnNickname, OwnEloRating, Match.ThirdTurnOrder);
        var secondRival = new FoundMatchParticipant(SecondRivalNickname, RivalEloRating, Match.SecondTurnOrder);
        FoundMatchParticipant[] participants = { thirdRival, rival, own, secondRival };
        var format = new FoundMatchFormat(BoardSize, FourPlayerWallsPerPlayer, ClockMinutes);
        return Create(format, participants);
    }

    private static JoinedMatch Create(FoundMatchFormat format, FoundMatchParticipant[] participants)
    {
        var session = new LoggedInSession(SessionToken, OwnNickname, PreferredLanguage);
        var matchFound = new MatchFoundEventArgs(MatchId, format, participants);
        return new JoinedMatch(session, matchFound);
    }
}
