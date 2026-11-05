using System;
using Bastion.Domain.Chat;
using Bastion.Server.Adapters.Persistence.Conversions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bastion.Server.Adapters.Persistence.Configurations;

public sealed class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    private const string TableName = "Message";
    private const int ChannelLength = 12;
    private const int TextLength = 500;

    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable(TableName);
        builder.HasKey(message => message.MessageId);
        builder.Property(message => message.Channel)
            .HasConversion(new UpperSnakeCaseEnumConverter<ChatChannel>())
            .HasMaxLength(ChannelLength)
            .IsUnicode(false);
        builder.Property(message => message.Text).HasMaxLength(TextLength);
    }
}
