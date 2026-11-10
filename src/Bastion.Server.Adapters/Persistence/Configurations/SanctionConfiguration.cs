using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Bastion.Domain.Moderation;
using Bastion.Server.Adapters.Persistence.Conversions;

namespace Bastion.Server.Adapters.Persistence.Configurations;

public sealed class SanctionConfiguration : IEntityTypeConfiguration<Sanction>
{
    private const int ScopeLength = 10;
    private const string EndsAtColumn = "EndedAt";

    public void Configure(EntityTypeBuilder<Sanction> builder)
    {
        builder.ToTable(nameof(Sanction));
        builder.HasKey(sanction => sanction.SanctionId);

        builder.Property(sanction => sanction.Scope)
            .HasConversion(new UpperSnakeCaseEnumConverter<SanctionScope>())
            .HasMaxLength(ScopeLength)
            .IsUnicode(false);
        builder.Property(sanction => sanction.EndsAt).HasColumnName(EndsAtColumn);
    }
}
