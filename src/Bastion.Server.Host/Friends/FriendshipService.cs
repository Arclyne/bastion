using System;
using System.Data.Common;
using Bastion.Contracts.Friends;
using Bastion.Server.Application.Friends;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Host.Sessions;
using CoreWCF;
using log4net;
using Microsoft.Extensions.DependencyInjection;

namespace Bastion.Server.Host.Friends;

[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
public sealed class FriendshipService : IFriendshipService
{
    private const string ServerFailureReason = "The server could not load the friend list.";

    private static readonly ILog _logger = LogManager.GetLogger(typeof(FriendshipService));

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;

    public FriendshipService(IServiceScopeFactory scopeFactory, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
    }

    public FriendList ListFriends(string? sessionToken)
    {
        if (sessionToken is null)
        {
            throw SessionTranslator.ToFault(new InvalidSessionException());
        }

        try
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            IFriendListService friendListService = scope.ServiceProvider.GetRequiredService<IFriendListService>();
            FriendRoster roster = friendListService.ListFriends(sessionToken, GetUtcNow());
            return FriendshipTranslator.ToFriendList(roster);
        }
        catch (InvalidSessionException ex)
        {
            throw SessionTranslator.ToFault(ex);
        }
        catch (DbException ex)
        {
            _logger.Error("The database could not be reached while loading a friend list.", ex);
            throw new FaultException(ServerFailureReason);
        }
    }

    private DateTime GetUtcNow()
    {
        return _timeProvider.GetUtcNow().UtcDateTime;
    }
}
