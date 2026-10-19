using System;
using Microsoft.EntityFrameworkCore;
using Bastion.Domain.Accounts;
using Bastion.Domain.Auditing;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BastionDbContext).Assembly);
    }
}
