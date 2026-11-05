using System;
using Bastion.Domain.Rooms;
using Bastion.Server.Adapters.Persistence.Conversions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bastion.Server.Adapters.Persistence.Configurations;

public sealed class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    private const int StatusLength = 10;

    public void Configure(EntityTypeBuilder<Room> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable(nameof(Room));
        builder.HasKey(room => room.RoomId);
        builder.Property(room => room.Code)
            .HasMaxLength(RoomRules.CodeLength)
            .IsFixedLength()
            .IsUnicode(false);
        builder.Property(room => room.Status)
            .HasConversion(new UpperSnakeCaseEnumConverter<RoomStatus>())
            .HasMaxLength(StatusLength)
            .IsUnicode(false);
    }
}
