using System;

namespace Bastion.Client.Networking.Chat;

public sealed record ChatMessage(ChatAuthor Author, string Text, DateTime SentAt);
