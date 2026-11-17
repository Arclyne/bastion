using System;
using Bastion.Domain.Friends;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bastion.Server.Adapters.Persistence.Configurations;

public sealed class FriendRequestConfiguration : IEntityTypeConfiguration<FriendRequest>
{
    public void Configure(EntityTypeBuilder<FriendRequest> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable(nameof(FriendRequest));
        builder.HasKey(request => new { request.RequesterId, request.RecipientId });
    }
}
