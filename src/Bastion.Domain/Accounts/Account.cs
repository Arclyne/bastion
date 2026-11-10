using System;

namespace Bastion.Domain.Accounts;

public class Account
{
    private const string DefaultPreferredLanguage = "en";

    public Account(string nickname, string email)
        : this()
    {
        ArgumentNullException.ThrowIfNull(nickname);
        ArgumentNullException.ThrowIfNull(email);

        Nickname = nickname;
        Email = email;
        AccountType = AccountType.Registered;
        AccountStatus = AccountStatus.Pending;
    }

    private Account()
    {
        Nickname = string.Empty;
        PreferredLanguage = DefaultPreferredLanguage;
        FriendCode = string.Empty;
    }

    public int AccountId { get; private set; }

    public AccountType AccountType { get; private set; }

    public AccountStatus AccountStatus { get; private set; }

    public string Nickname { get; private set; }

    public string? Email { get; private set; }

    public byte[]? PasswordHash { get; private set; }

    public byte[]? PasswordSalt { get; private set; }

    public string PreferredLanguage { get; private set; }

    public byte FailedSignInCount { get; private set; }

    public DateTime? LastFailedSignInAt { get; private set; }

    public DateTime? LockedUntil { get; private set; }

    public bool IsTwoFactorEnabled { get; private set; }

    public string FriendCode { get; private set; }

    public DateTime? LastSeenAt { get; private set; }

    public bool HasPassword()
    {
        return PasswordHash is not null && PasswordSalt is not null;
    }

    public void ChangePassword(byte[] hash, byte[] salt)
    {
        ArgumentNullException.ThrowIfNull(hash);
        ArgumentNullException.ThrowIfNull(salt);

        PasswordHash = hash;
        PasswordSalt = salt;
    }

    public void Activate()
    {
        if (AccountStatus != AccountStatus.Pending)
        {
            throw new InvalidOperationException("Only a pending account can be activated.");
        }

        AccountStatus = AccountStatus.Active;
    }

    public bool IsLockedAt(DateTime now)
    {
        return TryGetActiveLockEnd(now, out _);
    }

    public int GetRemainingLockMinutes(DateTime now)
    {
        if (!TryGetActiveLockEnd(now, out DateTime lockEnd))
        {
            return 0;
        }

        TimeSpan remainingLock = lockEnd - now;
        double roundedUpMinutes = Math.Ceiling(remainingLock.TotalMinutes);
        int remainingMinutes = (int)roundedUpMinutes;
        return remainingMinutes;
    }

    public void RecordFailedSignIn(DateTime now)
    {
        if (LastFailedSignInAt is DateTime lastFailure && now - lastFailure >= LockoutPolicy.CounterResetPeriod)
        {
            FailedSignInCount = 0;
        }

        if (FailedSignInCount < byte.MaxValue)
        {
            FailedSignInCount++;
        }

        LastFailedSignInAt = now;
        TimeSpan lockDuration = LockoutPolicy.GetLockDuration(FailedSignInCount);
        if (lockDuration > TimeSpan.Zero)
        {
            LockedUntil = now + lockDuration;
        }
    }

    public void RecordSuccessfulSignIn()
    {
        FailedSignInCount = 0;
        LastFailedSignInAt = null;
        LockedUntil = null;
    }

    public void RecordLastSeen(DateTime now)
    {
        LastSeenAt = now;
    }

    private bool TryGetActiveLockEnd(DateTime now, out DateTime lockEnd)
    {
        if (LockedUntil is DateTime lockedUntil && lockedUntil > now)
        {
            lockEnd = lockedUntil;
            return true;
        }

        lockEnd = default;
        return false;
    }
}
