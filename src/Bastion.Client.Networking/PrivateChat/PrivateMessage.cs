using System;

namespace Bastion.Client.Networking.PrivateChat;

public sealed record PrivateMessage(string Text, DateTime SentAt, bool IsOwn);
