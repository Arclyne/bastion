using System;
using Bastion.Domain.Matches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bastion.Server.Adapters.Persistence.Configurations;

public sealed class ParticipationConfiguration : IEntityTypeConfiguration<Participation>
{
    private const int PawnSymbolLength = 10;
    private const int SquareLength = 3;
    private const int ResultLength = 10;
    private const string RemainingClockColumn = "RemainingClock";

    public void Configure(EntityTypeBuilder<Participation> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable(nameof(Participation));
        builder.HasKey(nameof(Participation.MatchId), nameof(Participation.AccountId));
        builder.Property(participation => participation.PawnSymbol).HasUpperSnakeCaseConversion(PawnSymbolLength);
        builder.Property(participation => participation.CurrentSquare).HasMaxLength(SquareLength).IsUnicode(false);
        builder.Property(participation => participation.RemainingClockMilliseconds).HasColumnName(RemainingClockColumn);
        builder.Property(participation => participation.Result).HasUpperSnakeCaseConversion(ResultLength);
        builder.Property(participation => participation.EndReason).HasUpperSnakeCaseConversion(MatchConfiguration.EndReasonLength);
    }
}
