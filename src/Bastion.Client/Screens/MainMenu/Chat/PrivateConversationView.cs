using System;
using System.Collections.Specialized;
using System.ComponentModel;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed class PrivateConversationView : StackPanel
{
    private const float SectionSpacing = 10;
    private const float ScrollHeight = 360;
    private const float RowSpacing = 8;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(PrivateConversationView));

    private readonly PrivateConversationViewModel _conversation;
    private readonly ItemsControl _messages;

    public PrivateConversationView(PrivateConversationViewModel conversation)
    {
        ArgumentNullException.ThrowIfNull(conversation);

        _conversation = conversation;
        BindingContext = conversation;
        Width = ConversationPartFactory.ViewWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Spacing = SectionSpacing;
        _messages = CreateMessages();
        StackPanel header = CreateHeader();
        Label greeting = ConversationPartFactory.CreateNoticeLabel(
            nameof(PrivateConversationViewModel.GreetingText),
            nameof(PrivateConversationViewModel.IsGreetingVisible),
            TextStyleCatalog.NoticeTitle);
        Label greetingHint = CreateGreetingHint();
        Label loadingLabel = ViewFactory.CreateToggledLabel(
            nameof(Strings.ChatLoading),
            nameof(PrivateConversationViewModel.IsLoading),
            TextStyleCatalog.Hint);
        StackPanel loadFailure = CreateLoadFailure();
        Label offlineNotice = ConversationPartFactory.CreateNoticeLabel(
            nameof(PrivateConversationViewModel.OfflineText),
            nameof(PrivateConversationViewModel.IsOfflineNoticeVisible),
            TextStyleCatalog.Hint);
        Label notFriendsNotice = CreateNotFriendsNotice();
        Label muteNotice = ConversationPartFactory.CreateNoticeLabel(
            nameof(PrivateConversationViewModel.MuteText),
            nameof(PrivateConversationViewModel.IsMuteNoticeVisible),
            TextStyleCatalog.FieldError);
        Label statusNotice = ConversationPartFactory.CreateNoticeLabel(
            nameof(PrivateConversationViewModel.StatusText),
            nameof(PrivateConversationViewModel.HasStatus),
            TextStyleCatalog.FieldError);
        Label counterNotice = ConversationPartFactory.CreateNoticeLabel(
            nameof(PrivateConversationViewModel.CounterText),
            nameof(PrivateConversationViewModel.IsCounterVisible),
            TextStyleCatalog.Hint);
        StackPanel inputRow = ConversationPartFactory.CreateInputRow(
            HandleSendClick,
            ConversationPartFactory.ViewWidth);

        AddChild(header);
        AddChild(greeting);
        AddChild(greetingHint);
        AddChild(loadingLabel);
        AddChild(loadFailure);
        AddChild(_messages);
        AddChild(offlineNotice);
        AddChild(notFriendsNotice);
        AddChild(muteNotice);
        AddChild(statusNotice);
        AddChild(counterNotice);
        AddChild(inputRow);

        _conversation.Messages.CollectionChanged += HandleMessagesChanged;
        _conversation.PropertyChanged += HandleConversationPropertyChanged;
    }

    private ItemsControl CreateMessages()
    {
        ItemsControl messages = ConversationPartFactory.CreateMessageList(ScrollHeight, RowSpacing);
        messages.VisualTemplate = PrivateMessageRowView.CreateTemplate(ConversationPartFactory.MessageRowWidth);
        messages.Items = _conversation.Messages;
        return messages;
    }

    private StackPanel CreateHeader()
    {
        Label title = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(PrivateConversationViewModel.PartnerNickname),
            TextStyleCatalog.IdentityName);
        Label detail = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(PrivateConversationViewModel.PresenceText),
            TextStyleCatalog.Hint);
        return ConversationPartFactory.CreateHeader(HandleBackClick, title, detail);
    }

    private void HandleBackClick(object? sender, EventArgs e)
    {
        _conversation.Close();
    }

    private static Label CreateGreetingHint()
    {
        Label hint = ConversationPartFactory.CreateWrappedLabel(
            nameof(Strings.PrivateGreetingHint),
            TextStyleCatalog.Hint);
        hint.SetBinding(nameof(IsVisible), nameof(PrivateConversationViewModel.IsGreetingVisible));
        return hint;
    }

    private StackPanel CreateLoadFailure()
    {
        StackPanel failure = ViewFactory.CreateLoadFailure(
            nameof(Strings.ChatLoadFailed),
            nameof(Strings.ChatRetry),
            HandleRetryClick);
        failure.SetBinding(nameof(IsVisible), nameof(PrivateConversationViewModel.HasLoadFailed));
        return failure;
    }

    private async void HandleRetryClick(object? sender, EventArgs e)
    {
        try
        {
            await _conversation.Load();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The private conversation could not be reloaded because the connection is closed.", ex);
        }
    }

    private static Label CreateNotFriendsNotice()
    {
        Label notice = ConversationPartFactory.CreateWrappedLabel(
            nameof(Strings.PrivateNotFriends),
            TextStyleCatalog.FieldError);
        notice.SetBinding(nameof(IsVisible), nameof(PrivateConversationViewModel.IsNotFriendsNoticeVisible));
        return notice;
    }

    private async void HandleSendClick(object? sender, EventArgs e)
    {
        try
        {
            await _conversation.SendMessage();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The private message could not be sent because the connection is closed.", ex);
        }
    }

    private void HandleMessagesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ConversationPartFactory.ScrollToLatest(_messages);
    }

    private void HandleConversationPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PrivateConversationViewModel.IsLoading) && !_conversation.IsLoading)
        {
            ConversationPartFactory.ScrollToLatest(_messages);
        }
    }
}
