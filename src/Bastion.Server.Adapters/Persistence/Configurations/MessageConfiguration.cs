using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Bastion.Domain.Chat;
using Bastion.Server.Adapters.Persistence.Conversions;

namespace Bastion.Server.Adapters.Persistence.Configurations;

public sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    private const int ChannelLength = 12;
    private const int TextLength = 500;
    private const int TimePrecision = 3;

    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable(nameof(Message));
        builder.HasKey(message => message.MessageId);

        builder.Property(message => message.Channel)
            .HasConversion(new UpperSnakeCaseEnumConverter<MessageChannel>())
            .HasMaxLength(ChannelLength)
            .IsUnicode(false);
        builder.Property(message => message.Text).HasMaxLength(TextLength);
        builder.Property(message => message.SentAt).HasPrecision(TimePrecision);
        builder.Property(message => message.ReadAt).HasPrecision(TimePrecision);
    }
}
