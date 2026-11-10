using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Bastion.Domain.Chat;

namespace Bastion.Server.Adapters.Persistence.Configurations;

public sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    private const int ChannelLength = 12;
    private const int TimePrecision = 3;

    public void Configure(EntityTypeBuilder<Message> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable(nameof(Message));
        builder.HasKey(message => message.MessageId);

        builder.Property(message => message.Channel).HasUpperSnakeCaseConversion(ChannelLength);
        builder.Property(message => message.Text).HasMaxLength(ChatPolicy.MaximumMessageLength);
        builder.Property(message => message.SentAt).HasPrecision(TimePrecision);
        builder.Property(message => message.ReadAt).HasPrecision(TimePrecision);
    }
}
