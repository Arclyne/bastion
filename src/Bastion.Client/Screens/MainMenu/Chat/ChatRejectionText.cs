using System;
using System.Globalization;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Chat;
using Bastion.Domain.Chat;

namespace Bastion.Client.Screens.MainMenu.Chat;

public static class ChatRejectionText
{
    public static string DescribeInvalidMessage()
    {
        return string.Format(
            CultureInfo.CurrentCulture,
            Strings.ChatRejectedInvalidFormat,
            ChatPolicy.MaximumMessageLength);
    }

    public static string Describe(ChatRejection rejection)
    {
        switch (rejection)
        {
            case ChatRejection.InvalidMessage:
                return DescribeInvalidMessage();
            case ChatRejection.Muted:
                return Strings.ChatRejectedMuted;
            case ChatRejection.BannedTerm:
                return Strings.ChatRejectedBannedTerm;
            case ChatRejection.TooFast:
                return Strings.ChatRejectedTooFast;
            case ChatRejection.NotFriends:
                return Strings.ChatRejectedNotFriends;
            case ChatRejection.PlayerNotFound:
                return Strings.ChatRejectedPlayerNotFound;
            default:
                throw new ArgumentOutOfRangeException(nameof(rejection), rejection, "Unsupported chat rejection.");
        }
    }

    public static string DescribeMute(ChatMute mute)
    {
        ArgumentNullException.ThrowIfNull(mute);

        if (!mute.IsMuted)
        {
            return string.Empty;
        }

        if (mute.EndsAt is not DateTime endsAt)
        {
            return Strings.ChatMutedPermanently;
        }

        string localEnd = ChatTimeText.FormatMoment(endsAt);
        return string.Format(CultureInfo.CurrentCulture, Strings.ChatMutedUntilFormat, localEnd);
    }
}
