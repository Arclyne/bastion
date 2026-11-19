using System;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Rooms;
using Bastion.Client.Screens.MainMenu.Rooms;
using Bastion.Client.Tests.Fakes;
using Xunit;

namespace Bastion.Client.Tests.Screens.MainMenu.Rooms;

public sealed class TestJoinRoomDialogViewModel : IDisposable
{
    private const string LowercaseCode = "k7mtq3";
    private const string CodeWithDash = "K7M-TQ3";
    private const string LongCode = "K7MTQ3XY";
    private const string CodeWithConfusingLetter = "K7MTQO";
    private const string ShortCode = "K7M";

    private static readonly DateTimeOffset _now = new DateTimeOffset(2026, 11, 19, 18, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(45);

    private readonly ManualTimeProvider _timeProvider = new ManualTimeProvider(_now);
    private readonly JoinRoomDialogViewModel _dialog;

    public TestJoinRoomDialogViewModel()
    {
        Func<JoinRoomDialogViewModel> createDialog = () => new JoinRoomDialogViewModel(_timeProvider);
        _dialog = InlineUiContext.CreateViewModel(createDialog);
    }

    public void Dispose()
    {
        _dialog.Dispose();
    }

    [Fact]
    public void CodeText_Lowercase_TurnsUppercase()
    {
        _dialog.CodeText = LowercaseCode;

        Assert.Equal(PrivateRoomSampleFactory.Code, _dialog.CodeText);
    }

    [Fact]
    public void CodeText_WithDash_DropsTheDash()
    {
        _dialog.CodeText = CodeWithDash;

        Assert.Equal(PrivateRoomSampleFactory.Code, _dialog.CodeText);
    }

    [Fact]
    public void CodeText_TooLong_KeepsSixCharacters()
    {
        _dialog.CodeText = LongCode;

        Assert.Equal(PrivateRoomSampleFactory.Code, _dialog.CodeText);
    }

    [Fact]
    public void CanEnter_ShortCode_IsFalse()
    {
        _dialog.CodeText = ShortCode;

        bool canEnter = _dialog.CanEnter;

        Assert.False(canEnter);
    }

    [Fact]
    public void AcceptCode_ConfusingLetter_SaysTheFormat()
    {
        _dialog.CodeText = CodeWithConfusingLetter;

        _dialog.AcceptCode();

        Assert.Equal(Strings.RoomJoinInvalid, _dialog.ErrorText);
    }

    [Fact]
    public void ShowRejection_TooManyAttempts_TurnsOffEnter()
    {
        _dialog.CodeText = PrivateRoomSampleFactory.Code;

        _dialog.ShowRejection(RoomRejection.TooManyAttempts, _retryDelay);

        Assert.False(_dialog.CanEnter);
    }

    [Fact]
    public void ShowRejection_TooManyAttemptsAndTheWaitPassed_TurnsEnterBackOn()
    {
        _dialog.CodeText = PrivateRoomSampleFactory.Code;
        _dialog.ShowRejection(RoomRejection.TooManyAttempts, _retryDelay);

        _timeProvider.Advance(_retryDelay);

        Assert.True(_dialog.CanEnter);
    }

    [Fact]
    public void ShowRejection_RoomFull_SaysTheRoomIsFull()
    {
        _dialog.ShowRejection(RoomRejection.RoomFull, TimeSpan.Zero);

        Assert.Equal(Strings.RoomJoinFull, _dialog.ErrorText);
    }
}
