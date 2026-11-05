using System;
using Bastion.Domain.Accounts;
using Bastion.Domain.Auditing;
using Bastion.Domain.Chat;
using Bastion.Domain.GameModes;
using Bastion.Domain.Matches;
using Bastion.Domain.Sessions;
using Bastion.Domain.Statistics;
using Microsoft.EntityFrameworkCore;

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

    public DbSet<GameMode> GameModes => Set<GameMode>();

    public DbSet<Match> Matches => Set<Match>();

    public DbSet<Participation> Participations => Set<Participation>();

    public DbSet<ModeStatistic> ModeStatistics => Set<ModeStatistic>();

    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BastionDbContext).Assembly);
    }
}
