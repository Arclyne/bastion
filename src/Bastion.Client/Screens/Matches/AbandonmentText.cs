using System;
using System.Globalization;
using System.Linq;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Matches;
using Bastion.Client.Networking.Matchmaking;
using Bastion.Domain.Matches;

namespace Bastion.Client.Screens.Matches;

public static class AbandonmentText
{
    public static string DescribePendingConsequence(JoinedMatch joinedMatch)
    {
        ArgumentNullException.ThrowIfNull(joinedMatch);

        if (joinedMatch.FoundMatch.Participants.Count != Match.TwoPlayerCount)
        {
            return Strings.MatchAbandonConfirmPending;
        }

        return DescribeDuelConsequence(joinedMatch);
    }

    public static string DescribeConsequence(JoinedMatch joinedMatch, PreviewedAbandonment preview)
    {
        ArgumentNullException.ThrowIfNull(joinedMatch);
        ArgumentNullException.ThrowIfNull(preview);

        if (preview.Placement is not byte placement)
        {
            return DescribeDuelConsequence(joinedMatch);
        }

        return string.Format(
            CultureInfo.CurrentCulture,
            Strings.MatchAbandonConfirmGroupFormat,
            placement,
            preview.ActivePlayerCount);
    }

    public static string FormatEloLoss(short initialElo, short finalElo)
    {
        int eloLoss = initialElo - finalElo;
        return string.Format(
            CultureInfo.CurrentCulture,
            Strings.MatchAbandonEloLossFormat,
            eloLoss,
            initialElo,
            finalElo);
    }

    private static string DescribeDuelConsequence(JoinedMatch joinedMatch)
    {
        FoundMatchParticipant? rival = joinedMatch.FoundMatch.Participants.FirstOrDefault(
            participant => !joinedMatch.IsOwnParticipant(participant));
        string rivalNickname = rival is null ? string.Empty : rival.Nickname;
        return string.Format(CultureInfo.CurrentCulture, Strings.MatchAbandonConfirmDuelFormat, rivalNickname);
    }
}
