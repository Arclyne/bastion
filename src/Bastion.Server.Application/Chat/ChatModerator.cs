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

        ChatMuteStatus muteStatus = FindMuteStatus(accountId, now);
        if (muteStatus.IsMuted)
        {
            throw new ChatMessageRejectedException(muteStatus);
        }

        string trimmedText = text.Trim();
        if (ChatWordFilter.HasBannedTerm(trimmedText, _unitOfWork.BannedWords.ListChatTerms()))
        {
            throw new ChatMessageRejectedException(ChatRejection.BannedTerm);
        }

        return trimmedText;
    }

    public ChatMuteStatus FindMuteStatus(int accountId, DateTime now)
    {
        IReadOnlyList<Sanction> chatSanctions = _unitOfWork.Sanctions.ListChatSanctions(accountId);
        IEnumerable<Sanction> currentSanctions = chatSanctions.Where(sanction => sanction.IsInEffectAt(now));
        List<Sanction> sanctionsInEffect = [.. currentSanctions];
        return ChatMuteStatus.FromSanctionsInEffect(sanctionsInEffect);
    }
}
