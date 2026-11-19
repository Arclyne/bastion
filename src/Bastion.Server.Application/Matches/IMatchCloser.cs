using System;
using Bastion.Domain.Matches;

namespace Bastion.Server.Application.Matches;

public interface IMatchCloser
{
    void CloseAfterMove(Match finishedMatch);

    void CloseWithoutMove(Match finishedMatch);

    void AnnounceEnd(Match finishedMatch, int decidingAccountId);

    void AnnounceDeparture(Match match, int departingAccountId, DateTime now);

    void ExpireClock(Match match, DateTime now);

    void TakeOutAbsentPlayer(Match match, DateTime now);

    MatchEndNotice DescribeEnd(Match finishedMatch, int decidingAccountId);

    ParticipationEndNotice DescribeDeparture(Match match, int departingAccountId, DateTime now);
}
