using System;
using Bastion.Domain.Matches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bastion.Server.Adapters.Persistence.Configurations;

public sealed class DrawOfferConfiguration : IEntityTypeConfiguration<DrawOffer>
{
    private const int StatusLength = 10;

    public void Configure(EntityTypeBuilder<DrawOffer> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable(nameof(DrawOffer));
        builder.HasKey(drawOffer => drawOffer.DrawOfferId);
        builder.Property(drawOffer => drawOffer.Status).HasUpperSnakeCaseConversion(StatusLength);
    }
}
