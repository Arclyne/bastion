using System;
using Bastion.Domain.GameModes;
using Bastion.Server.Adapters.Persistence.Conversions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bastion.Server.Adapters.Persistence.Configurations;

public sealed class GameModeConfiguration : IEntityTypeConfiguration<GameMode>
{
    private const int CodeLength = 40;

    public void Configure(EntityTypeBuilder<GameMode> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable(nameof(GameMode));
        builder.HasKey(gameMode => gameMode.GameModeId);
        builder.Property(gameMode => gameMode.GameModeId).ValueGeneratedNever();
        builder.Property(gameMode => gameMode.Code)
            .HasConversion(new UpperSnakeCaseEnumConverter<GameModeCode>())
            .HasMaxLength(CodeLength)
            .IsUnicode(false);
    }
}
