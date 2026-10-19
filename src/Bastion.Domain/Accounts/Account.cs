using System;

namespace Bastion.Domain.Accounts;

public class Account
{
    public const string DefaultLanguage = "en";

    public Account(string nickname, string email)
    {
        ArgumentNullException.ThrowIfNull(nickname);
        ArgumentNullException.ThrowIfNull(email);

        Nickname = nickname;
        Email = email;
        AccountType = AccountType.Registered;
        AccountStatus = AccountStatus.Pending;
        PreferredLanguage = DefaultLanguage;
    }

    private Account()
    {
        Nickname = string.Empty;
        PreferredLanguage = DefaultLanguage;
    }

    public int AccountId { get; private set; }

    public AccountType AccountType { get; private set; }

    public AccountStatus AccountStatus { get; private set; }

    public string Nickname { get; private set; }

    public string? Email { get; private set; }

    public byte[]? PasswordHash { get; private set; }

    public byte[]? PasswordSalt { get; private set; }

    public string PreferredLanguage { get; private set; }

    public byte FailedAttempts { get; private set; }

    public DateTime? LastFailedAttemptAt { get; private set; }

    public DateTime? LockedUntil { get; private set; }

    public bool IsTwoFactorEnabled { get; private set; }

    public bool CanSignIn => AccountType == AccountType.Registered && AccountStatus == AccountStatus.Active;

    public bool HasPassword => PasswordHash is not null && PasswordSalt is not null;

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
        return LockedUntil is DateTime lockedUntil && lockedUntil > now;
    }

    public int GetRemainingLockMinutes(DateTime now)
    {
        if (LockedUntil is not DateTime lockedUntil || lockedUntil <= now)
        {
            return 0;
        }

        return (int)Math.Ceiling((lockedUntil - now).TotalMinutes);
    }

    public void RecordFailedSignIn(DateTime now)
    {
        if (LastFailedAttemptAt is DateTime lastFailure && now - lastFailure >= LockoutPolicy.CounterResetPeriod)
        {
            FailedAttempts = 0;
        }

        if (FailedAttempts < byte.MaxValue)
        {
            FailedAttempts++;
        }

        LastFailedAttemptAt = now;
        TimeSpan lockDuration = LockoutPolicy.GetLockDuration(FailedAttempts);
        if (lockDuration > TimeSpan.Zero)
        {
            LockedUntil = now + lockDuration;
        }
    }

    public void RecordSuccessfulSignIn()
    {
        FailedAttempts = 0;
        LastFailedAttemptAt = null;
        LockedUntil = null;
    }
}
