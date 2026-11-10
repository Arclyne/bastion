using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Bastion.Domain.Accounts;
using Bastion.Domain.Auditing;
using Bastion.Domain.Sessions;

namespace Bastion.Server.Adapters.Persistence.Configurations;

public sealed class AccessLogConfiguration : IEntityTypeConfiguration<AccessLog>
{
    private const int ResultLength = 40;

    public void Configure(EntityTypeBuilder<AccessLog> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable(nameof(AccessLog));
        builder.HasKey(accessLog => accessLog.AccessLogId);
        builder.Property(accessLog => accessLog.EnteredIdentifier).HasMaxLength(AccountPolicy.MaximumLoginIdentifierLength);
        builder.Property(accessLog => accessLog.Result).HasUpperSnakeCaseConversion(ResultLength);
        builder.Property(accessLog => accessLog.IpAddress).HasMaxLength(ClientConnection.MaximumIpAddressLength).IsUnicode(false);
    }
}
