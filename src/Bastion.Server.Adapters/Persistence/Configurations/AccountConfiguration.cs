using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Bastion.Domain.Accounts;
using Bastion.Domain.Friends;
using Bastion.Server.Adapters.Persistence.Conversions;
using Bastion.Server.Adapters.Security;

namespace Bastion.Server.Adapters.Persistence.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    private const int TypeLength = 10;
    private const int StatusLength = 10;
    private const int LanguageLength = 10;
    private const string FailedLoginCountColumn = "FailedAttempts";
    private const string LastFailedLoginAtColumn = "LastFailedAttemptAt";

    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable(nameof(Account));
        builder.HasKey(account => account.AccountId);

        builder.Property(account => account.AccountType)
            .HasConversion(new UpperSnakeCaseEnumConverter<AccountType>())
            .HasMaxLength(TypeLength)
            .IsUnicode(false);
        builder.Property(account => account.AccountStatus)
            .HasConversion(new UpperSnakeCaseEnumConverter<AccountStatus>())
            .HasMaxLength(StatusLength)
            .IsUnicode(false);

        builder.Property(account => account.Nickname).HasMaxLength(AccountPolicy.MaximumNicknameLength);
        builder.Property(account => account.Email).HasMaxLength(AccountPolicy.MaximumEmailLength);
        builder.Property(account => account.PasswordHash).HasMaxLength(PasswordHasher.HashLength);
        builder.Property(account => account.PasswordSalt).HasMaxLength(PasswordHasher.SaltLength);
        builder.Property(account => account.PreferredLanguage).HasMaxLength(LanguageLength).IsUnicode(false);
        builder.Property(account => account.FailedLoginCount).HasColumnName(FailedLoginCountColumn);
        builder.Property(account => account.LastFailedLoginAt).HasColumnName(LastFailedLoginAtColumn);
        builder.Property(account => account.FriendCode)
            .HasMaxLength(FriendCodeRules.Length)
            .IsFixedLength()
            .IsUnicode(false);
    }
}
