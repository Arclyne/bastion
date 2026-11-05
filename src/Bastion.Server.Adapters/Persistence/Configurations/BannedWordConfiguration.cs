using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Bastion.Domain.Moderation;
using Bastion.Server.Adapters.Persistence.Conversions;

namespace Bastion.Server.Adapters.Persistence.Configurations;

public sealed class BannedWordConfiguration : IEntityTypeConfiguration<BannedWord>
{
    private const int TermLength = 100;
    private const int ScopeLength = 10;

    public void Configure(EntityTypeBuilder<BannedWord> builder)
    {
        builder.ToTable(nameof(BannedWord));
        builder.HasKey(bannedWord => bannedWord.BannedWordId);

        builder.Property(bannedWord => bannedWord.Term).HasMaxLength(TermLength);
        builder.Property(bannedWord => bannedWord.Scope)
            .HasConversion(new UpperSnakeCaseEnumConverter<BannedWordScope>())
            .HasMaxLength(ScopeLength)
            .IsUnicode(false);
    }
}
