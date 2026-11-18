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
    private const float ScrollHeight = 360;
    private const float RowSpacing = 8;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(PrivateConversationView));

    private readonly PrivateConversationViewModel _conversation;
    private readonly ItemsControl _messagesItemsControl;

    public PrivateConversationView(PrivateConversationViewModel conversation)
    {
        ArgumentNullException.ThrowIfNull(conversation);

        _conversation = conversation;
        BindingContext = conversation;
        Width = ConversationPartFactory.ViewWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Spacing = PanelMetrics.SectionSpacing;
        _messagesItemsControl = CreateMessages();
        StackPanel headerStackPanel = CreateHeader();
        Label greetingLabel = ConversationPartFactory.CreateNoticeLabel(
            nameof(PrivateConversationViewModel.GreetingText),
            nameof(PrivateConversationViewModel.IsGreetingVisible),
            TextStyleCatalog.NoticeTitle);
        Label greetingHintLabel = CreateToggledWrappedLabel(
            nameof(Strings.PrivateGreetingHint),
            nameof(PrivateConversationViewModel.IsGreetingVisible),
            TextStyleCatalog.Hint);
        Label loadingLabel = ViewFactory.CreateToggledLabel(
            nameof(Strings.ChatLoading),
            nameof(PrivateConversationViewModel.IsLoading),
            TextStyleCatalog.Hint);
        StackPanel loadFailureStackPanel = CreateLoadFailure();
        Label offlineNoticeLabel = ConversationPartFactory.CreateNoticeLabel(
            nameof(PrivateConversationViewModel.OfflineText),
            nameof(PrivateConversationViewModel.IsOfflineNoticeVisible),
            TextStyleCatalog.Hint);
        Label notFriendsNoticeLabel = CreateToggledWrappedLabel(
            nameof(Strings.PrivateNotFriends),
            nameof(PrivateConversationViewModel.IsNotFriendsNoticeVisible),
            TextStyleCatalog.FieldError);
        Label muteNoticeLabel = ConversationPartFactory.CreateNoticeLabel(
            nameof(PrivateConversationViewModel.MuteText),
            nameof(PrivateConversationViewModel.IsMuteNoticeVisible),
            TextStyleCatalog.FieldError);
        StackPanel composerStackPanel = ConversationPartFactory.CreateComposer(HandleSendClick);

        AddChild(headerStackPanel);
        AddChild(greetingLabel);
        AddChild(greetingHintLabel);
        AddChild(loadingLabel);
        AddChild(loadFailureStackPanel);
        AddChild(_messagesItemsControl);
        AddChild(offlineNoticeLabel);
        AddChild(notFriendsNoticeLabel);
        AddChild(muteNoticeLabel);
        AddChild(composerStackPanel);

        _conversation.Messages.CollectionChanged += HandleMessagesChanged;
        _conversation.PropertyChanged += HandleConversationPropertyChanged;
    }

    private ItemsControl CreateMessages()
    {
        ItemsControl messagesItemsControl = ConversationPartFactory.CreateMessageList(ScrollHeight, RowSpacing);
        messagesItemsControl.VisualTemplate = PrivateMessageRowView.CreateTemplate(
            ConversationPartFactory.MessageRowWidth);
        messagesItemsControl.Items = _conversation.Messages;
        return messagesItemsControl;
    }

    private StackPanel CreateHeader()
    {
        Label titleLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(PrivateConversationViewModel.PartnerNickname),
            TextStyleCatalog.IdentityName);
        Label detailLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(PrivateConversationViewModel.PresenceText),
            TextStyleCatalog.Hint);
        return ConversationPartFactory.CreateHeader(HandleBackClick, titleLabel, detailLabel);
    }

    private void HandleBackClick(object? sender, EventArgs e)
    {
        _conversation.Close();
    }

    private static Label CreateToggledWrappedLabel(string textKey, string visibilityProperty, TextStyle style)
    {
        Label label = ConversationPartFactory.CreateWrappedLabel(textKey, style);
        label.SetBinding(nameof(IsVisible), visibilityProperty);
        return label;
    }

    private StackPanel CreateLoadFailure()
    {
        StackPanel failureStackPanel = ConversationPartFactory.CreateLoadFailure(
            HandleRetryClick,
            nameof(PrivateConversationViewModel.HasLoadFailed));
        return failureStackPanel;
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
        ConversationPartFactory.ScrollToLatest(_messagesItemsControl);
    }

    private void HandleConversationPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PrivateConversationViewModel.IsLoading) && !_conversation.IsLoading)
        {
            ConversationPartFactory.ScrollToLatest(_messagesItemsControl);
        }
    }
}
