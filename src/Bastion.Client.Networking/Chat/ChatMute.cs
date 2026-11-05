using System;

namespace Bastion.Client.Networking.Chat;

public sealed record ChatMute(bool IsMuted, DateTime? EndsAt);
