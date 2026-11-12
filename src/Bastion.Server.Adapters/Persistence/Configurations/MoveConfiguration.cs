using System;
using Bastion.Domain.Matches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bastion.Server.Adapters.Persistence.Configurations;

public sealed class MoveConfiguration : IEntityTypeConfiguration<Move>
{
    private const int MoveTypeLength = 10;
    private const int SquareLength = 3;
    private const string TimeSpentColumn = "TimeSpent";

    public void Configure(EntityTypeBuilder<Move> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable(nameof(Move));
        builder.HasKey(nameof(Move.MatchId), nameof(Move.MoveNumber));
        builder.Property(move => move.MoveNumber).ValueGeneratedNever();
        builder.Property(move => move.MoveType).HasUpperSnakeCaseConversion(MoveTypeLength);
        builder.Property(move => move.FromSquare).HasMaxLength(SquareLength).IsUnicode(false);
        builder.Property(move => move.ToSquare).HasMaxLength(SquareLength).IsUnicode(false);
        builder.Property(move => move.TimeSpentMilliseconds).HasColumnName(TimeSpentColumn);
    }
}
