using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Bastion.Domain.Friends;

namespace Bastion.Server.Adapters.Persistence.Configurations;

public sealed class FriendRequestConfiguration : IEntityTypeConfiguration<FriendRequest>
{
    public void Configure(EntityTypeBuilder<FriendRequest> builder)
    {
        builder.ToTable(nameof(FriendRequest));
        builder.HasKey(request => new { request.RequesterId, request.RecipientId });
        builder.Ignore(request => request.ExpiresAt);
    }
}
