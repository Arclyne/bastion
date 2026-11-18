using System;
using Bastion.Domain.Matches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bastion.Server.Adapters.Persistence.Configurations;

public sealed class MoveConfiguration : IEntityTypeConfiguration<Move>
{
    public const int SquareLength = 3;

    private const int MoveTypeLength = 10;
    private const int OrientationLength = 10;
    private const string TimeSpentColumn = "TimeSpent";

    public void Configure(EntityTypeBuilder<Move> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable(nameof(Move));
        builder.HasKey(playedMove => new { playedMove.MatchId, playedMove.MoveNumber });
        builder.Property(playedMove => playedMove.MoveNumber).ValueGeneratedNever();
        builder.Property(playedMove => playedMove.MoveType).HasUpperSnakeCaseConversion(MoveTypeLength);
        builder.Property(playedMove => playedMove.FromSquare).HasMaxLength(SquareLength).IsUnicode(false);
        builder.Property(playedMove => playedMove.ToSquare).HasMaxLength(SquareLength).IsUnicode(false);
        builder.Property(playedMove => playedMove.Groove).HasMaxLength(SquareLength).IsUnicode(false);
        builder.Property(playedMove => playedMove.Orientation).HasUpperSnakeCaseConversion(OrientationLength);
        builder.Property(playedMove => playedMove.TimeSpentMilliseconds).HasColumnName(TimeSpentColumn);
    }
}
