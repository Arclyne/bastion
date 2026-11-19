using System;
using System.Collections.Generic;
using System.Globalization;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Rooms;

namespace Bastion.Client.Screens.MainMenu.Rooms;

public static class RoomRejectionText
{
    private static readonly IReadOnlyDictionary<RoomRejection, Func<string>> _textsByRejection =
        new Dictionary<RoomRejection, Func<string>>
        {
            [RoomRejection.NotificationsUnavailable] = () => Strings.RoomNotificationsUnavailable,
            [RoomRejection.UnfinishedMatch] = () => Strings.RoomBusyMatch,
            [RoomRejection.SearchingMatch] = () => Strings.RoomBusySearching,
            [RoomRejection.InRoom] = () => Strings.RoomBusyInRoom,
            [RoomRejection.RoomNotFound] = () => Strings.RoomJoinNotFound,
            [RoomRejection.RoomFull] = () => Strings.RoomJoinFull,
            [RoomRejection.Kicked] = () => Strings.RoomJoinKicked,
            [RoomRejection.MatchStarted] = () => Strings.RoomJoinStarted,
            [RoomRejection.TooManyPlayers] = () => Strings.RoomTooManyPlayers,
            [RoomRejection.NotReadyToStart] = () => Strings.RoomNotReadyToStart,
        };

    public static string Describe(RoomRejection rejection)
    {
        bool isKnownRejection = _textsByRejection.TryGetValue(rejection, out Func<string>? readText);
        return isKnownRejection && readText is not null ? readText() : Strings.RoomRequestFailed;
    }

    public static string DescribeRetryDelay(TimeSpan retryDelay)
    {
        string waitText = retryDelay.ToString(@"m\:ss", CultureInfo.CurrentCulture);
        return string.Format(CultureInfo.CurrentCulture, Strings.RoomJoinTooManyFormat, waitText);
    }
}
