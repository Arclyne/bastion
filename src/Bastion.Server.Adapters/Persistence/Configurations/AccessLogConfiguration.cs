using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Bastion.Domain.Accounts;
using Bastion.Domain.Auditing;
using Bastion.Domain.Sessions;
using Bastion.Server.Adapters.Persistence.Conversions;

namespace Bastion.Server.Adapters.Persistence.Configurations;

public sealed class AccessLogConfiguration : IEntityTypeConfiguration<AccessLog>
{
    private const int ResultLength = 40;

    public void Configure(EntityTypeBuilder<AccessLog> builder)
    {
        builder.ToTable(nameof(AccessLog));
        builder.HasKey(accessLog => accessLog.AccessLogId);
        builder.Property(accessLog => accessLog.EnteredIdentifier).HasMaxLength(AccountRules.MaximumIdentifierLength);
        builder.Property(accessLog => accessLog.Result)
            .HasConversion(new UpperSnakeCaseEnumConverter<AccessLogResult>())
            .HasMaxLength(ResultLength)
            .IsUnicode(false);
        builder.Property(accessLog => accessLog.IpAddress).HasMaxLength(ClientConnection.MaximumIpAddressLength).IsUnicode(false);
    }
}
