using System;
using Bastion.Server.Application.Accounts;
using Bastion.Server.Application.Chat;
using Bastion.Server.Application.Common;
using Bastion.Server.Application.Friends;
using Bastion.Server.Application.Matches;
using Bastion.Server.Application.Matchmaking;
using Bastion.Server.Application.PrivateChat;
using Bastion.Server.Application.Rooms;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Host.Accounts;
using Bastion.Server.Host.Chat;
using Bastion.Server.Host.Friends;
using Bastion.Server.Host.Matches;
using Bastion.Server.Host.Matchmaking;
using Bastion.Server.Host.PrivateChat;
using Bastion.Server.Host.Rooms;
using Bastion.Server.Host.Sessions;
using CoreWCF.Configuration;
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
        services.AddScoped<IMatchChatService, MatchChatService>();
        services.AddScoped<IMatchAbandonmentService, MatchAbandonmentService>();
        services.AddScoped<IMatchMoveService, MatchMoveService>();
        services.AddScoped<IChatModerator, ChatModerator>();
        services.AddScoped<IMatchSearchService, MatchSearchService>();
        services.AddScoped<IMatchmakingPlayerVerifier, MatchmakingPlayerVerifier>();
        services.AddScoped<IFriendListService, FriendListService>();
        services.AddScoped<IFriendRequestService, FriendRequestService>();
        services.AddScoped<IFriendPresenceService, FriendPresenceService>();
        services.AddScoped<PrivateChatChannel>();
        services.AddScoped<IPrivateChatService, PrivateChatService>();
        services.AddScoped<IPrivateRoomService, PrivateRoomService>();
        services.AddScoped<IRoomPlayerVerifier, RoomPlayerVerifier>();
        services.AddScoped<IRoomSettingsValidator, RoomSettingsValidator>();
        services.AddScoped<IRoomMatchService, RoomMatchService>();
        services.AddSingleton<IMatchmakingQueue, MatchmakingQueue>();
        services.AddSingleton<IRoomRegistry, RoomRegistry>();
        services.AddSingleton<IRoomKeeper, RoomKeeper>();
        services.AddSingleton<IRoomMatchStarter, RoomMatchStarter>();
        services.AddSingleton<IPrivateMatchCreator, PrivateMatchCreator>();
        services.AddSingleton<IPlayerActivity, PlayerActivity>();
        services.AddSingleton<IRankedMatchCreator, RankedMatchCreator>();
        services.AddSingleton<ILobbyChatRoom, LobbyChatRoom>();
        services.AddSingleton<IMatchChatRelay, MatchChatRelay>();
        services.AddSingleton<IPrivateChatRoom, PrivateChatRoom>();
        services.AddSingleton<IMatchmakingDispatcher, MatchmakingDispatcher>();
        return services;
    }

    public static IServiceCollection AddBastionHost(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IFriendPresenceReporter, FriendPresenceReporter>();
        services.AddSingleton<NotificationChannelRegistry>();
        services.AddSingleton<INotificationChannelRegistry>(GetChannelRegistry);
        services.AddSingleton<IChatBroadcaster>(GetChannelRegistry);
        services.AddSingleton<IMatchChatNotifier>(GetChannelRegistry);
        services.AddSingleton<IMatchmakingNotifier>(GetChannelRegistry);
        services.AddSingleton<IMatchEndNotifier>(GetChannelRegistry);
        services.AddSingleton<IMatchMoveNotifier>(GetChannelRegistry);
        services.AddSingleton<IPlayerPresence>(GetChannelRegistry);
        services.AddSingleton<IFriendNotifier>(GetChannelRegistry);
        services.AddSingleton<IPrivateMessageNotifier>(GetChannelRegistry);
        services.AddSingleton<IRoomNotifier>(GetChannelRegistry);
        services.AddHostedService<MatchmakingBackgroundService>();
        services.AddHostedService<RoomCleanupService>();
        services.AddTransient<AccountService>();
        services.AddTransient<SessionService>();
        services.AddTransient<ChatService>();
        services.AddTransient<MatchmakingService>();
        services.AddTransient<MatchService>();
        services.AddTransient<FriendshipService>();
        services.AddTransient<ConversationService>();
        services.AddTransient<RoomService>();
        return services;
    }

    public static IServiceBuilder AddBastionServices(this IServiceBuilder serviceBuilder)
    {
        ArgumentNullException.ThrowIfNull(serviceBuilder);

        serviceBuilder.AddService<AccountService>();
        serviceBuilder.AddService<SessionService>();
        serviceBuilder.AddService<ChatService>();
        serviceBuilder.AddService<MatchmakingService>();
        serviceBuilder.AddService<MatchService>();
        serviceBuilder.AddService<FriendshipService>();
        serviceBuilder.AddService<ConversationService>();
        serviceBuilder.AddService<RoomService>();
        return serviceBuilder;
    }

    private static NotificationChannelRegistry GetChannelRegistry(IServiceProvider provider)
    {
        return provider.GetRequiredService<NotificationChannelRegistry>();
    }
}
