using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Bastion.Domain.Sessions;
using Bastion.Server.Adapters.Security;

namespace Bastion.Server.Adapters.Persistence.Configurations;

public sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    private const int DeviceFingerprintLength = 128;
    private const int ClientVersionLength = 20;

    public void Configure(EntityTypeBuilder<Session> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable(nameof(Session));
        builder.HasKey(session => session.SessionId);
        builder.Property(session => session.TokenHash).HasMaxLength(SessionTokenGenerator.TokenHashLength);
        builder.Property(session => session.IpAddress)
            .HasMaxLength(ClientConnection.MaximumIpAddressLength)
            .IsUnicode(false);
        builder.Property(session => session.DeviceFingerprint).HasMaxLength(DeviceFingerprintLength).IsUnicode(false);
        builder.Property(session => session.ClientVersion).HasMaxLength(ClientVersionLength).IsUnicode(false);
    }
}
