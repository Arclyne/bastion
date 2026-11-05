using System;
using Bastion.Server.Adapters.Matchmaking;
using Bastion.Server.Adapters.Persistence;
using Bastion.Server.Adapters.Security;
using Bastion.Server.Application.Common;
using Bastion.Server.Application.Matches;
using Bastion.Server.Application.Matchmaking;
using Bastion.Server.Application.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bastion.Server.Adapters;

public static class AdaptersRegistration
{
    public static IServiceCollection AddBastionAdapters(this IServiceCollection services, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        // The factory also registers the scoped context; the singleton match store opens its own contexts with it.
        services.AddDbContextFactory<BastionDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IMatchStore, MatchStore>();
        services.AddSingleton<ITurnOrderShuffler, TurnOrderShuffler>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ISessionTokenGenerator, SessionTokenGenerator>();
        return services;
    }
}
