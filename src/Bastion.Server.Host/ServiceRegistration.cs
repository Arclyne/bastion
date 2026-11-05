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
        services.AddScoped<IChatModerator, ChatModerator>();
        services.AddScoped<IMatchSearchService, MatchSearchService>();
        services.AddScoped<IMatchmakingPlayerVerifier, MatchmakingPlayerVerifier>();
        services.AddScoped<IFriendListService, FriendListService>();
        services.AddScoped<IFriendRequestService, FriendRequestService>();
        services.AddScoped<IFriendPresenceService, FriendPresenceService>();
        services.AddScoped<IPrivateChatService, PrivateChatService>();
        services.AddScoped<IPrivateRoomService, PrivateRoomService>();
        services.AddScoped<IRoomPlayerVerifier, RoomPlayerVerifier>();
        services.AddScoped<IRoomSettingsValidator, RoomSettingsValidator>();
        services.AddSingleton<FriendPresenceReporter>();
        services.AddSingleton<IMatchmakingQueue, MatchmakingQueue>();
        services.AddSingleton<IRoomRegistry, RoomRegistry>();
        services.AddSingleton<IRoomKeeper, RoomKeeper>();
        services.AddSingleton<IPlayerActivity, PlayerActivity>();
        services.AddSingleton<IRankedMatchCreator, RankedMatchCreator>();
        services.AddSingleton<ILobbyChatRoom, LobbyChatRoom>();
        services.AddSingleton<IMatchChatRelay, MatchChatRelay>();
        services.AddSingleton<IPrivateChatRoom, PrivateChatRoom>();
        services.AddSingleton<MatchmakingDispatcher>();
        services.AddSingleton<NotificationChannelRegistry>();
        services.AddSingleton<IChatBroadcaster>(provider => provider.GetRequiredService<NotificationChannelRegistry>());
        services.AddSingleton<IMatchChatNotifier>(
            provider => provider.GetRequiredService<NotificationChannelRegistry>());
        services.AddSingleton<IMatchmakingNotifier>(provider => provider.GetRequiredService<NotificationChannelRegistry>());
        services.AddSingleton<IMatchEndNotifier>(
            provider => provider.GetRequiredService<NotificationChannelRegistry>());
        services.AddSingleton<IPlayerPresence>(provider => provider.GetRequiredService<NotificationChannelRegistry>());
        services.AddSingleton<IFriendNotifier>(provider => provider.GetRequiredService<NotificationChannelRegistry>());
        services.AddSingleton<IPrivateMessageNotifier>(
            provider => provider.GetRequiredService<NotificationChannelRegistry>());
        services.AddSingleton<IRoomNotifier>(provider => provider.GetRequiredService<NotificationChannelRegistry>());
        services.AddHostedService<MatchmakingBackgroundService>();
        services.AddHostedService<RoomBackgroundService>();
        return services;
    }

    public static IServiceCollection AddBastionServiceModel(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton(TimeProvider.System);
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
}
