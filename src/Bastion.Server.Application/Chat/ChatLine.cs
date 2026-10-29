using System;

namespace Bastion.Server.Application.Chat;

public sealed record ChatLine(string SenderNickname, string Text, DateTime SentAt);
