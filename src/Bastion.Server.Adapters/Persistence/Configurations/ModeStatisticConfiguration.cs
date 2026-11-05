using System;
using Bastion.Domain.Statistics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bastion.Server.Adapters.Persistence.Configurations;

public sealed class ModeStatisticConfiguration : IEntityTypeConfiguration<ModeStatistic>
{
    public void Configure(EntityTypeBuilder<ModeStatistic> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable(nameof(ModeStatistic));
        builder.HasKey(nameof(ModeStatistic.AccountId), nameof(ModeStatistic.GameModeId));
    }
}
