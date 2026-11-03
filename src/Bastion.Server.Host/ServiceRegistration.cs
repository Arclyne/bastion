using System;
using Bastion.Server.Application.Accounts;
using Bastion.Server.Application.Chat;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Host.Accounts;
using Bastion.Server.Host.Chat;
using Bastion.Server.Host.Sessions;
using Microsoft.Extensions.DependencyInjection;

namespace Bastion.Server.Host;

public static class ServiceRegistration
{
    public static IServiceCollection AddBastionApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<ISessionVerifier, SessionVerifier>();
        services.AddScoped<ILobbyChatService, LobbyChatService>();
        services.AddSingleton<ConnectedPlayerRegistry>();
        services.AddSingleton<IChatBroadcaster>(provider => provider.GetRequiredService<ConnectedPlayerRegistry>());
        return services;
    }

    public static IServiceCollection AddBastionServiceModel(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton(TimeProvider.System);
        services.AddTransient<AccountService>();
        services.AddTransient<SessionService>();
        services.AddTransient<ChatService>();
        return services;
    }
}
