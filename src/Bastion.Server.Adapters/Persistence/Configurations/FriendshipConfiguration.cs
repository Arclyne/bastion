using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Bastion.Domain.Friends;

namespace Bastion.Server.Adapters.Persistence.Configurations;

public sealed class FriendshipConfiguration : IEntityTypeConfiguration<Friendship>
{
    public void Configure(EntityTypeBuilder<Friendship> builder)
    {
        builder.ToTable(nameof(Friendship));
        builder.HasKey(friendship => new { friendship.AccountAId, friendship.AccountBId });
    }
}
