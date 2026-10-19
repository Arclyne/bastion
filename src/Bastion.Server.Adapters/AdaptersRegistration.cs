using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Bastion.Server.Adapters.Persistence;
using Bastion.Server.Adapters.Security;
using Bastion.Server.Application.Common;
using Bastion.Server.Application.Security;

namespace Bastion.Server.Adapters;

public static class AdaptersRegistration
{
    public static IServiceCollection AddBastionAdapters(this IServiceCollection services, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<BastionDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ISessionTokenGenerator, SessionTokenGenerator>();
        return services;
    }
}
