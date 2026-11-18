using System;

namespace Bastion.Client.Networking.Chat;

public sealed record ChatMuteStatus(bool IsMuted, DateTime? EndsAt);
