using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Bastion.Domain.Auditing;
using Bastion.Server.Adapters.Persistence.Conversions;

namespace Bastion.Server.Adapters.Persistence.Configurations;

public sealed class AccessLogConfiguration : IEntityTypeConfiguration<AccessLog>
{
    private const int IdentifierLength = 254;
    private const int ResultLength = 40;
    private const int IpAddressLength = 45;

    public void Configure(EntityTypeBuilder<AccessLog> builder)
    {
        builder.ToTable(nameof(AccessLog));
        builder.HasKey(accessLog => accessLog.AccessLogId);
        builder.Property(accessLog => accessLog.EnteredIdentifier).HasMaxLength(IdentifierLength);
        builder.Property(accessLog => accessLog.Result)
            .HasConversion(new UpperSnakeCaseEnumConverter<AccessLogResult>())
            .HasMaxLength(ResultLength)
            .IsUnicode(false);
        builder.Property(accessLog => accessLog.IpAddress).HasMaxLength(IpAddressLength).IsUnicode(false);
    }
}
