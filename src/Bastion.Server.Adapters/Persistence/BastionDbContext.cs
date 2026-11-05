using System;
using Microsoft.EntityFrameworkCore;
using Bastion.Domain.Accounts;
using Bastion.Domain.Auditing;
using Bastion.Domain.Chat;
using Bastion.Domain.Friends;
using Bastion.Domain.Moderation;
using Bastion.Domain.Sessions;

namespace Bastion.Server.Adapters.Persistence;

public sealed class BastionDbContext : DbContext
{
    public BastionDbContext(DbContextOptions<BastionDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<Session> Sessions => Set<Session>();

    public DbSet<AccessLog> AccessLogs => Set<AccessLog>();

    public DbSet<Friendship> Friendships => Set<Friendship>();

    public DbSet<FriendRequest> FriendRequests => Set<FriendRequest>();

    public DbSet<Sanction> Sanctions => Set<Sanction>();

    public DbSet<BannedWord> BannedWords => Set<BannedWord>();

    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BastionDbContext).Assembly);
    }
}
