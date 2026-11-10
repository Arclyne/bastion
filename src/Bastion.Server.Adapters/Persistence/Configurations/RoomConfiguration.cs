using System;
using Bastion.Domain.Rooms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bastion.Server.Adapters.Persistence.Configurations;

public sealed class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    private const int StatusLength = 10;
    private const string HostAccountIdColumn = "HostId";

    public void Configure(EntityTypeBuilder<Room> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable(nameof(Room));
        builder.HasKey(room => room.RoomId);
        builder.Property(room => room.HostAccountId).HasColumnName(HostAccountIdColumn);
        builder.Property(room => room.Code)
            .HasMaxLength(RoomPolicy.CodeLength)
            .IsFixedLength()
            .IsUnicode(false);
        builder.Property(room => room.Status).HasUpperSnakeCaseConversion(StatusLength);
    }
}
