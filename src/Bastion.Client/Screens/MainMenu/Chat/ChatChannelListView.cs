using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
using Gum.GueDeriving;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed class ChatChannelListView : StackPanel
{
    private const float ListWidth = 360;
    private const float ListSpacing = 12;
    private const float RowHeight = 64;
    private const float RowRadius = 10;
    private const float RowPaddingX = 14;
    private const float TextSpacing = 2;
    private const float OpenButtonWidth = 88;
    private const float OpenButtonHeight = 36;
    private const float RetryButtonWidth = 140;
    private const float RetryButtonHeight = 40;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(ChatChannelListView));

    private readonly LobbyChatViewModel _chat;

    public ChatChannelListView(LobbyChatViewModel chat)
    {
        ArgumentNullException.ThrowIfNull(chat);

        _chat = chat;
        Width = ListWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Spacing = ListSpacing;
        AddChild(CreateChannelRow());
        AddChild(CreateLoadFailure());
    }

    private static StackPanel CreateChannelText()
    {
        Label detail = ViewFactory.CreateLabel(string.Empty, TextStyles.Hint);
        ((LabelVisual)detail.Visual).LocalizeText = false;
        detail.SetBinding(nameof(Label.Text), nameof(LobbyChatViewModel.ChannelDetail));

        var text = new StackPanel { Spacing = TextSpacing, X = RowPaddingX };
        text.AddChild(ViewFactory.CreateLabel(nameof(Strings.ChatGlobalChannel), TextStyles.IdentityName));
        text.AddChild(detail);
        ViewFactory.CenterVertically(text);
        return text;
    }

    private Panel CreateChannelRow()
    {
        Button open = ViewFactory.CreateButton(nameof(Strings.ChatOpen), OpenButtonWidth, OpenButtonHeight);
        ButtonSkin.CreateAndAttach(open, ButtonLooks.Secondary);
        open.Anchor(Gum.Wireframe.Anchor.Right);
        open.X = -RowPaddingX;
        open.Click += HandleOpenClick;

        var row = new Panel
        {
            Width = ListWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = RowHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        RectangleRuntime background = Shapes.CreateRounded(Palette.Selected, RowRadius);
        Shapes.InsertBehindContent(row.Visual, background);
        row.AddChild(CreateChannelText());
        row.AddChild(open);
        return row;
    }

    private StackPanel CreateLoadFailure()
    {
        Button retry = ViewFactory.CreateButton(nameof(Strings.ChatRetry), RetryButtonWidth, RetryButtonHeight);
        ButtonSkin.CreateAndAttach(retry, ButtonLooks.Secondary);
        retry.Click += HandleRetryClick;

        var failure = new StackPanel { Spacing = ListSpacing };
        failure.AddChild(ViewFactory.CreateLabel(nameof(Strings.ChatLoadFailed), TextStyles.FieldError));
        failure.AddChild(retry);
        failure.SetBinding(nameof(IsVisible), nameof(LobbyChatViewModel.HasLoadFailed));
        return failure;
    }

    private void HandleOpenClick(object? sender, EventArgs e)
    {
        _chat.OpenConversation();
    }

    private async void HandleRetryClick(object? sender, EventArgs e)
    {
        try
        {
            await _chat.LoadChannel();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The lobby chat could not be reloaded because the connection is closed.", ex);
        }
    }
}
