using System;
using Bastion.Domain.Friends;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bastion.Server.Adapters.Persistence.Configurations;

public sealed class FriendshipConfiguration : IEntityTypeConfiguration<Friendship>
{
    private const string SmallerAccountIdColumn = "AccountAId";
    private const string LargerAccountIdColumn = "AccountBId";

    public void Configure(EntityTypeBuilder<Friendship> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable(nameof(Friendship));
        builder.HasKey(friendship => new { friendship.SmallerAccountId, friendship.LargerAccountId });
        builder.Property(friendship => friendship.SmallerAccountId).HasColumnName(SmallerAccountIdColumn);
        builder.Property(friendship => friendship.LargerAccountId).HasColumnName(LargerAccountIdColumn);
    }
}
