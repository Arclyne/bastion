using System;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Chat;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed class ChatMessageRowViewModel : MessageRowViewModel
{
    public ChatMessageRowViewModel(ChatMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        IsOwn = message.Author.IsOwn;
        Text = message.Text;
        AuthorText = IsOwn ? Strings.ChatOwnAuthor : message.Author.Nickname;
        ShowSentTime(message.SentAt);
    }

    public string AuthorText { get; }

    public static ChatMessageRowViewModel CreatePending(string text, DateTime createdAt)
    {
        ArgumentNullException.ThrowIfNull(text);

        var author = new ChatAuthor(string.Empty, isOwn: true);
        var message = new ChatMessage(author, text, createdAt);
        var row = new ChatMessageRowViewModel(message);
        row.MarkPending();
        return row;
    }
}
