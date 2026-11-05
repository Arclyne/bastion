using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using Bastion.Client.Controls;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed class PrivateConversationView : StackPanel
{
    private const float ViewWidth = 360;
    private const float SectionSpacing = 10;
    private const float HeaderSpacing = 12;
    private const float TextSpacing = 2;
    private const float BackButtonWidth = 96;
    private const float BackButtonHeight = 36;
    private const float ScrollHeight = 360;
    private const float RowSpacing = 8;
    private const float InputHeight = 44;
    private const float SendButtonWidth = 96;
    private const float InputSpacing = 8;
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
        Width = ViewWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Spacing = SectionSpacing;
        _messages = CreateMessages();

        AddChild(CreateHeader());
        AddChild(CreateBoundLabel(
            nameof(PrivateConversationViewModel.GreetingText),
            nameof(PrivateConversationViewModel.IsGreetingVisible),
            TextStyles.NoticeTitle));
        AddChild(CreateGreetingHint());
        AddChild(CreateVisibleLabel(nameof(Strings.ChatLoading), nameof(PrivateConversationViewModel.IsLoading)));
        AddChild(CreateLoadFailure());
        AddChild(_messages);
        AddChild(CreateBoundLabel(
            nameof(PrivateConversationViewModel.OfflineText),
            nameof(PrivateConversationViewModel.IsOfflineNoticeVisible),
            TextStyles.Hint));
        AddChild(CreateNotFriendsNotice());
        AddChild(CreateBoundLabel(
            nameof(PrivateConversationViewModel.MuteText),
            nameof(PrivateConversationViewModel.IsMuteNoticeVisible),
            TextStyles.FieldError));
        AddChild(CreateBoundLabel(
            nameof(PrivateConversationViewModel.StatusText),
            nameof(PrivateConversationViewModel.HasStatus),
            TextStyles.FieldError));
        AddChild(CreateBoundLabel(
            nameof(PrivateConversationViewModel.CounterText),
            nameof(PrivateConversationViewModel.IsCounterVisible),
            TextStyles.Hint));
        AddChild(CreateInputRow());

        _conversation.Messages.CollectionChanged += HandleMessagesChanged;
        _conversation.PropertyChanged += HandleConversationPropertyChanged;
    }

    private static Label CreateWrappedLabel(string textKey, TextStyle style)
    {
        Label label = ViewFactory.CreateLabel(textKey, style);
        label.Width = ViewWidth;
        label.WidthUnits = DimensionUnitType.Absolute;
        return label;
    }

    private static Label CreateNotFriendsNotice()
    {
        Label notice = CreateWrappedLabel(nameof(Strings.PrivateNotFriends), TextStyles.FieldError);
        notice.SetBinding(nameof(IsVisible), nameof(PrivateConversationViewModel.IsNotFriendsNoticeVisible));
        return notice;
    }

    private static Label CreateBoundLabel(string textProperty, string visibilityProperty, TextStyle style)
    {
        Label label = CreateWrappedLabel(string.Empty, style);
        ((LabelVisual)label.Visual).LocalizeText = false;
        label.SetBinding(nameof(Label.Text), textProperty);
        label.SetBinding(nameof(IsVisible), visibilityProperty);
        return label;
    }

    private static Label CreateVisibleLabel(string textKey, string visibilityProperty)
    {
        Label label = ViewFactory.CreateLabel(textKey, TextStyles.Hint);
        label.SetBinding(nameof(IsVisible), visibilityProperty);
        return label;
    }

    private static Label CreateGreetingHint()
    {
        Label hint = CreateWrappedLabel(nameof(Strings.PrivateGreetingHint), TextStyles.Hint);
        hint.SetBinding(nameof(IsVisible), nameof(PrivateConversationViewModel.IsGreetingVisible));
        return hint;
    }

    private static ScrollViewer CreateMessages()
    {
        var messages = new ScrollViewer
        {
            Width = ViewWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = ScrollHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        Shapes.HideDefaultParts(messages);
        messages.InnerPanel.StackSpacing = RowSpacing;
        return messages;
    }

    private StackPanel CreateHeader()
    {
        Button back = ViewFactory.CreateButton(nameof(Strings.ChatBack), BackButtonWidth, BackButtonHeight);
        ButtonSkin.CreateAndAttach(back, ButtonLooks.Pill);
        back.Click += HandleBackClick;

        Label nickname = ViewFactory.CreateLabel(string.Empty, TextStyles.IdentityName);
        ((LabelVisual)nickname.Visual).LocalizeText = false;
        nickname.SetBinding(nameof(Label.Text), nameof(PrivateConversationViewModel.PartnerNickname));
        Label presence = ViewFactory.CreateLabel(string.Empty, TextStyles.Hint);
        ((LabelVisual)presence.Visual).LocalizeText = false;
        presence.SetBinding(nameof(Label.Text), nameof(PrivateConversationViewModel.PresenceText));

        var title = new StackPanel { Spacing = TextSpacing };
        title.AddChild(nickname);
        title.AddChild(presence);

        var header = new StackPanel { Orientation = Orientation.Horizontal, Spacing = HeaderSpacing };
        header.AddChild(back);
        header.AddChild(title);
        return header;
    }

    private StackPanel CreateLoadFailure()
    {
        Button retry = ViewFactory.CreateButton(nameof(Strings.ChatRetry), RetryButtonWidth, RetryButtonHeight);
        ButtonSkin.CreateAndAttach(retry, ButtonLooks.Secondary);
        retry.Click += HandleRetryClick;

        var failure = new StackPanel { Spacing = SectionSpacing };
        failure.AddChild(ViewFactory.CreateLabel(nameof(Strings.ChatLoadFailed), TextStyles.FieldError));
        failure.AddChild(retry);
        failure.SetBinding(nameof(IsVisible), nameof(PrivateConversationViewModel.HasLoadFailed));
        return failure;
    }

    private StackPanel CreateInputRow()
    {
        var input = new TextField
        {
            Placeholder = nameof(Strings.ChatPlaceholder),
            Width = ViewWidth - SendButtonWidth - InputSpacing,
            Height = InputHeight,
        };
        input.SetBinding(nameof(TextField.Text), nameof(PrivateConversationViewModel.Draft));
        input.SetBinding(nameof(IsEnabled), nameof(PrivateConversationViewModel.CanWrite));

        Button send = ViewFactory.CreateButton(nameof(Strings.ChatSend), SendButtonWidth, InputHeight);
        ButtonSkin.CreateAndAttach(send, ButtonLooks.Primary);
        send.SetBinding(nameof(IsEnabled), nameof(PrivateConversationViewModel.CanSend));
        send.Click += HandleSendClick;

        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = InputSpacing };
        row.AddChild(input);
        row.AddChild(send);
        return row;
    }

    private void HandleMessagesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            AddRows(e.NewItems);
        }
        else
        {
            RebuildRows();
        }

        ScrollToLatest();
    }

    private void AddRows(IList? rows)
    {
        if (rows is null)
        {
            return;
        }

        foreach (PrivateMessageRowViewModel row in rows)
        {
            _messages.InnerPanel.AddChild(new PrivateMessageRowView(row).Visual);
        }
    }

    private void RebuildRows()
    {
        while (_messages.InnerPanel.Children.Count > 0)
        {
            _messages.InnerPanel.Children.RemoveAt(_messages.InnerPanel.Children.Count - 1);
        }

        AddRows(_conversation.Messages);
    }

    private void ScrollToLatest()
    {
        _messages.VerticalScrollBarValue = _messages.VerticalScrollBarMaximum;
    }

    private void HandleConversationPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PrivateConversationViewModel.IsLoading) && !_conversation.IsLoading)
        {
            ScrollToLatest();
        }
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
}
