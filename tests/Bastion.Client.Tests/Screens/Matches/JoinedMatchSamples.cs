using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Screens.Matches;
using Bastion.Domain.Matches;

namespace Bastion.Client.Tests.Screens.Matches;

public static class JoinedMatchSamples
{
    public const int MatchId = 12;
    public const string OwnNickname = "marta_muros";
    public const string RivalNickname = "leo_peon";
    public const short OwnEloRating = 1210;
    public const short RivalEloRating = 1000;
    public const byte BoardSize = 9;
    public const byte WallsPerPlayer = 10;
    public const byte ClockMinutes = 5;
    public const string ThirdNickname = "luis_quo";
    public const string FourthNickname = "sofia_salto";
    public const byte FourPlayerWallsPerPlayer = 5;

    private const string SessionToken = "session-token";
    private const string PreferredLanguage = "es-MX";

    public static JoinedMatch CreateWithOwnTurnOrder(byte ownTurnOrder)
    {
        byte rivalTurnOrder = ownTurnOrder == Match.FirstTurnOrder ? Match.SecondTurnOrder : Match.FirstTurnOrder;

        var participants = new FoundMatchParticipant[]
        {
            new FoundMatchParticipant(RivalNickname, RivalEloRating, rivalTurnOrder),
            new FoundMatchParticipant(OwnNickname, OwnEloRating, ownTurnOrder),
        };
        var format = new FoundMatchFormat(BoardSize, WallsPerPlayer, ClockMinutes);
        var session = new LoggedInSession(SessionToken, OwnNickname, PreferredLanguage);
        return new JoinedMatch(session, new MatchFoundEventArgs(MatchId, format, participants));
    }

    public static JoinedMatch CreateFourPlayerWithOwnThirdTurn()
    {
        var participants = new FoundMatchParticipant[]
        {
            new FoundMatchParticipant(FourthNickname, RivalEloRating, Match.FourthTurnOrder),
            new FoundMatchParticipant(RivalNickname, RivalEloRating, Match.FirstTurnOrder),
            new FoundMatchParticipant(OwnNickname, OwnEloRating, Match.ThirdTurnOrder),
            new FoundMatchParticipant(ThirdNickname, RivalEloRating, Match.SecondTurnOrder),
        };
        var format = new FoundMatchFormat(BoardSize, FourPlayerWallsPerPlayer, ClockMinutes);
        var session = new LoggedInSession(SessionToken, OwnNickname, PreferredLanguage);
        return new JoinedMatch(session, new MatchFoundEventArgs(MatchId, format, participants));
    }
}
