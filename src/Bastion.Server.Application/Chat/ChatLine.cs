using System;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Chat;

public sealed record ChatLine(ConnectedPlayer Sender, string Text, DateTime SentAt);
