using System;
using System.Collections.Specialized;
using System.ComponentModel;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms;
using Gum.Forms.Controls;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed class PrivateConversationView : StackPanel
{
    private const float SectionSpacing = 10;
    private const float ScrollHeight = 360;
    private const float RowSpacing = 8;
    private const float RetryButtonWidth = 140;
    private const float RetryButtonHeight = 40;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(PrivateConversationView));

    private readonly PrivateConversationViewModel _conversation;
    private readonly ItemsControl _messages;

    public PrivateConversationView(PrivateConversationViewModel conversation)
    {
        ArgumentNullException.ThrowIfNull(conversation);

        _conversation = conversation;
        BindingContext = conversation;
        Width = ConversationParts.ViewWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Spacing = SectionSpacing;
        _messages = CreateMessages();

        AddChild(CreateHeader());
        AddChild(ConversationParts.CreateNoticeLabel(
            nameof(PrivateConversationViewModel.GreetingText),
            nameof(PrivateConversationViewModel.IsGreetingVisible),
            TextStyleCatalog.NoticeTitle));
        AddChild(CreateGreetingHint());
        AddChild(CreateVisibleLabel(nameof(Strings.ChatLoading), nameof(PrivateConversationViewModel.IsLoading)));
        AddChild(CreateLoadFailure());
        AddChild(_messages);
        AddChild(ConversationParts.CreateNoticeLabel(
            nameof(PrivateConversationViewModel.OfflineText),
            nameof(PrivateConversationViewModel.IsOfflineNoticeVisible),
            TextStyleCatalog.Hint));
        AddChild(CreateNotFriendsNotice());
        AddChild(ConversationParts.CreateNoticeLabel(
            nameof(PrivateConversationViewModel.MuteText),
            nameof(PrivateConversationViewModel.IsMuteNoticeVisible),
            TextStyleCatalog.FieldError));
        AddChild(ConversationParts.CreateNoticeLabel(
            nameof(PrivateConversationViewModel.StatusText),
            nameof(PrivateConversationViewModel.HasStatus),
            TextStyleCatalog.FieldError));
        AddChild(ConversationParts.CreateNoticeLabel(
            nameof(PrivateConversationViewModel.CounterText),
            nameof(PrivateConversationViewModel.IsCounterVisible),
            TextStyleCatalog.Hint));
        AddChild(ConversationParts.CreateInputRow(HandleSendClick, ConversationParts.ViewWidth));

        _conversation.Messages.CollectionChanged += HandleMessagesChanged;
        _conversation.PropertyChanged += HandleConversationPropertyChanged;
    }

    private ItemsControl CreateMessages()
    {
        ItemsControl messages = ConversationParts.CreateMessageList(ScrollHeight, RowSpacing);
        messages.VisualTemplate = new VisualTemplate(PrivateMessageRowView.CreateVisual);
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
        return ConversationParts.CreateHeader(HandleBackClick, title, detail);
    }

    private static Label CreateGreetingHint()
    {
        Label hint = ConversationParts.CreateWrappedLabel(nameof(Strings.PrivateGreetingHint), TextStyleCatalog.Hint);
        hint.SetBinding(nameof(IsVisible), nameof(PrivateConversationViewModel.IsGreetingVisible));
        return hint;
    }

    private static Label CreateVisibleLabel(string textKey, string visibilityProperty)
    {
        Label label = ViewFactory.CreateLabel(textKey, TextStyleCatalog.Hint);
        label.SetBinding(nameof(IsVisible), visibilityProperty);
        return label;
    }

    private StackPanel CreateLoadFailure()
    {
        Button retry = ViewFactory.CreateButton(nameof(Strings.ChatRetry), RetryButtonWidth, RetryButtonHeight);
        ButtonSkin.CreateAndAttach(retry, ButtonLookCatalog.Secondary);
        retry.Click += HandleRetryClick;

        var failure = new StackPanel
        {
            Spacing = SectionSpacing,
        };
        failure.AddChild(ViewFactory.CreateLabel(nameof(Strings.ChatLoadFailed), TextStyleCatalog.FieldError));
        failure.AddChild(retry);
        failure.SetBinding(nameof(IsVisible), nameof(PrivateConversationViewModel.HasLoadFailed));
        return failure;
    }

    private static Label CreateNotFriendsNotice()
    {
        Label notice = ConversationParts.CreateWrappedLabel(nameof(Strings.PrivateNotFriends), TextStyleCatalog.FieldError);
        notice.SetBinding(nameof(IsVisible), nameof(PrivateConversationViewModel.IsNotFriendsNoticeVisible));
        return notice;
    }

    private void HandleBackClick(object? sender, EventArgs e)
    {
        _conversation.Close();
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
        ConversationParts.ScrollToLatest(_messages);
    }

    private void HandleConversationPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PrivateConversationViewModel.IsLoading) && !_conversation.IsLoading)
        {
            ConversationParts.ScrollToLatest(_messages);
        }
    }
}
