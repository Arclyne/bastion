using System;
using System.Linq;
using Bastion.Domain.Chat;
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

    public string Screen(int accountId, string text, DateTime now)
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
        var sanctionsInEffect = _unitOfWork.Sanctions.ListChatSanctions(accountId)
            .Where(sanction => sanction.IsInEffectAt(now))
            .ToList();
        return ChatMute.FromSanctionsInEffect(sanctionsInEffect);
    }
}
