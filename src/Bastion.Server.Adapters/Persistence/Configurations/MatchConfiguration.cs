using System;
using Bastion.Domain.Matches;
using Bastion.Server.Adapters.Persistence.Conversions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bastion.Server.Adapters.Persistence.Configurations;

public sealed class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    private const int MatchTypeLength = 15;
    private const int StatusLength = 20;
    private const int EndReasonLength = 15;
    private const string CurrentTurnOrderColumn = "CurrentTurn";

    public void Configure(EntityTypeBuilder<Match> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable(nameof(Match));
        builder.HasKey(match => match.MatchId);
        builder.Property(match => match.MatchType)
            .HasConversion(new UpperSnakeCaseEnumConverter<MatchType>())
            .HasMaxLength(MatchTypeLength)
            .IsUnicode(false);
        builder.Property(match => match.Status)
            .HasConversion(new UpperSnakeCaseEnumConverter<MatchStatus>())
            .HasMaxLength(StatusLength)
            .IsUnicode(false);
        builder.Property(match => match.EndReason)
            .HasConversion(new UpperSnakeCaseEnumConverter<MatchEndReason>())
            .HasMaxLength(EndReasonLength)
            .IsUnicode(false);
        builder.Property(match => match.CurrentTurnOrder).HasColumnName(CurrentTurnOrderColumn);
        builder.HasMany(match => match.Participations)
            .WithOne()
            .HasForeignKey(participation => participation.MatchId);
        builder.Navigation(match => match.Participations).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
