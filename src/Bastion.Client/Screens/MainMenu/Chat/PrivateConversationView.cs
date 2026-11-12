using System;
using System.Collections;
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
    private const float RetryButtonWidth = 140;
    private const float RetryButtonHeight = 40;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(PrivateConversationView));

    private readonly PrivateConversationViewModel _conversation;
    private readonly ScrollViewer _messages;

    public PrivateConversationView(PrivateConversationViewModel conversation)
    {
        ArgumentNullException.ThrowIfNull(conversation);

        _conversation = conversation;
        BindingContext = conversation;
        Width = ConversationParts.ViewWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Spacing = SectionSpacing;
        _messages = ConversationParts.CreateMessageList(ScrollHeight, RowSpacing);

        AddChild(CreateHeader());
        AddChild(ConversationParts.CreateNoticeLabel(
            nameof(PrivateConversationViewModel.GreetingText),
            nameof(PrivateConversationViewModel.IsGreetingVisible),
            TextStyles.NoticeTitle));
        AddChild(CreateGreetingHint());
        AddChild(CreateVisibleLabel(nameof(Strings.ChatLoading), nameof(PrivateConversationViewModel.IsLoading)));
        AddChild(CreateLoadFailure());
        AddChild(_messages);
        AddChild(ConversationParts.CreateNoticeLabel(
            nameof(PrivateConversationViewModel.OfflineText),
            nameof(PrivateConversationViewModel.IsOfflineNoticeVisible),
            TextStyles.Hint));
        AddChild(CreateNotFriendsNotice());
        AddChild(ConversationParts.CreateNoticeLabel(
            nameof(PrivateConversationViewModel.MuteText),
            nameof(PrivateConversationViewModel.IsMuteNoticeVisible),
            TextStyles.FieldError));
        AddChild(ConversationParts.CreateNoticeLabel(
            nameof(PrivateConversationViewModel.StatusText),
            nameof(PrivateConversationViewModel.HasStatus),
            TextStyles.FieldError));
        AddChild(ConversationParts.CreateNoticeLabel(
            nameof(PrivateConversationViewModel.CounterText),
            nameof(PrivateConversationViewModel.IsCounterVisible),
            TextStyles.Hint));
        AddChild(ConversationParts.CreateInputRow(HandleSendClick));

        _conversation.Messages.CollectionChanged += HandleMessagesChanged;
        _conversation.PropertyChanged += HandleConversationPropertyChanged;
    }

    private StackPanel CreateHeader()
    {
        Label title = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(PrivateConversationViewModel.PartnerNickname),
            TextStyles.IdentityName);
        Label detail = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(PrivateConversationViewModel.PresenceText),
            TextStyles.Hint);
        return ConversationParts.CreateHeader(HandleBackClick, title, detail);
    }

    private static Label CreateGreetingHint()
    {
        Label hint = ConversationParts.CreateWrappedLabel(nameof(Strings.PrivateGreetingHint), TextStyles.Hint);
        hint.SetBinding(nameof(IsVisible), nameof(PrivateConversationViewModel.IsGreetingVisible));
        return hint;
    }

    private static Label CreateVisibleLabel(string textKey, string visibilityProperty)
    {
        Label label = ViewFactory.CreateLabel(textKey, TextStyles.Hint);
        label.SetBinding(nameof(IsVisible), visibilityProperty);
        return label;
    }

    private StackPanel CreateLoadFailure()
    {
        Button retry = ViewFactory.CreateButton(nameof(Strings.ChatRetry), RetryButtonWidth, RetryButtonHeight);
        ButtonSkin.CreateAndAttach(retry, ButtonLooks.Secondary);
        retry.Click += HandleRetryClick;

        var failure = new StackPanel
        {
            Spacing = SectionSpacing,
        };
        failure.AddChild(ViewFactory.CreateLabel(nameof(Strings.ChatLoadFailed), TextStyles.FieldError));
        failure.AddChild(retry);
        failure.SetBinding(nameof(IsVisible), nameof(PrivateConversationViewModel.HasLoadFailed));
        return failure;
    }

    private static Label CreateNotFriendsNotice()
    {
        Label notice = ConversationParts.CreateWrappedLabel(nameof(Strings.PrivateNotFriends), TextStyles.FieldError);
        notice.SetBinding(nameof(IsVisible), nameof(PrivateConversationViewModel.IsNotFriendsNoticeVisible));
        return notice;
    }

    private void AddRows(IList? rows)
    {
        if (rows is null)
        {
            return;
        }

        foreach (PrivateMessageRowViewModel row in rows)
        {
            var rowView = new PrivateMessageRowView(row);
            _messages.InnerPanel.AddChild(rowView.Visual);
        }
    }

    private void RebuildRows()
    {
        ViewFactory.ClearRows(_messages.InnerPanel);
        AddRows(_conversation.Messages);
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
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                AddRows(e.NewItems);
                break;
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Replace:
            case NotifyCollectionChangedAction.Move:
            case NotifyCollectionChangedAction.Reset:
                RebuildRows();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(e), e.Action, "Unsupported collection change.");
        }

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
