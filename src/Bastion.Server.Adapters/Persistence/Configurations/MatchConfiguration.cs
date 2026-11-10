using System;
using Bastion.Domain.Matches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bastion.Server.Adapters.Persistence.Configurations;

public sealed class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public const int EndReasonLength = 15;

    private const int MatchTypeLength = 15;
    private const int StatusLength = 20;
    private const string CurrentTurnOrderColumn = "CurrentTurn";

    public void Configure(EntityTypeBuilder<Match> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable(nameof(Match));
        builder.HasKey(match => match.MatchId);
        builder.Property(match => match.MatchType).HasUpperSnakeCaseConversion(MatchTypeLength);
        builder.Property(match => match.Status).HasUpperSnakeCaseConversion(StatusLength);
        builder.Property(match => match.EndReason).HasUpperSnakeCaseConversion(EndReasonLength);
        builder.Property(match => match.CurrentTurnOrder).HasColumnName(CurrentTurnOrderColumn);
        builder.HasMany(match => match.Participations)
            .WithOne()
            .HasForeignKey(participation => participation.MatchId);
        builder.Navigation(match => match.Participations).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
