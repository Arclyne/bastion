using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Chat;
using Bastion.Domain.Moderation;
using Bastion.Server.Application.Common;

namespace Bastion.Server.Application.Chat;

public sealed class ChatModerator : IChatModerator
{
    private readonly IUnitOfWork _unitOfWork;

    public ChatModerator(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);

        _unitOfWork = unitOfWork;
    }

    public string ScreenMessage(int accountId, string text, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (!ChatPolicy.IsValidMessage(text))
        {
            throw new ChatMessageRejectedException(ChatRejection.InvalidMessage);
        }

        ChatMute mute = FindMute(accountId, now);
        if (mute.IsMuted)
        {
            throw new ChatMessageRejectedException(mute);
        }

        string trimmedText = text.Trim();
        if (ChatWordFilter.HasBannedTerm(trimmedText, _unitOfWork.BannedWords.ListChatTerms()))
        {
            throw new ChatMessageRejectedException(ChatRejection.BannedTerm);
        }

        return trimmedText;
    }

    public ChatMute FindMute(int accountId, DateTime now)
    {
        IReadOnlyList<Sanction> chatSanctions = _unitOfWork.Sanctions.ListChatSanctions(accountId);
        IEnumerable<Sanction> sanctionsInEffect = chatSanctions.Where(sanction => sanction.IsInEffectAt(now));
        var sanctionList = new List<Sanction>(sanctionsInEffect);
        return ChatMute.FromSanctionsInEffect(sanctionList);
    }
}
