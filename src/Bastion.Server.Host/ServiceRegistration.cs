using System;
using Microsoft.Extensions.DependencyInjection;
using Bastion.Server.Application.Accounts;
using Bastion.Server.Host.Accounts;

namespace Bastion.Server.Host;

public static class ServiceRegistration
{
    public static IServiceCollection AddBastionApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<ILoginService, LoginService>();
        return services;
    }

    public static IServiceCollection AddBastionServiceModel(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient<AccountService>();
        return services;
    }
}
