using System;
using System.Data.Common;
using Bastion.Contracts.Friends;
using Bastion.Server.Application.Friends;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Host.Sessions;
using CoreWCF;
using log4net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bastion.Server.Host.Friends;

[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
public sealed class FriendshipService : IFriendshipService
{
    private const string ServerFailureReason = "The server could not complete the friend operation.";

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
        string token = RequireSessionToken(sessionToken);
        return Run(nameof(ListFriends), services =>
        {
            FriendRoster roster = services.GetRequiredService<IFriendListService>().ListFriends(token, GetUtcNow());
            return FriendshipTranslator.ToFriendList(roster);
        });
    }

    public PlayerSearchEntry[] SearchPlayers(string? sessionToken, string? text)
    {
        string token = RequireSessionToken(sessionToken);
        string searchText = text ?? string.Empty;
        return Run(nameof(SearchPlayers), services =>
        {
            IFriendRequestService requestService = services.GetRequiredService<IFriendRequestService>();
            return FriendshipTranslator.ToSearchEntries(requestService.SearchPlayers(token, searchText, GetUtcNow()));
        });
    }

    public FriendRequestResult SendFriendRequest(string? sessionToken, string? friendCode)
    {
        string token = RequireSessionToken(sessionToken);
        if (friendCode is null)
        {
            throw FriendshipTranslator.ToFault(new FriendshipRejectedException(FriendshipRejection.PlayerNotFound));
        }

        return Run(nameof(SendFriendRequest), services =>
        {
            IFriendRequestService requestService = services.GetRequiredService<IFriendRequestService>();
            return FriendshipTranslator.ToResult(requestService.SendFriendRequest(token, friendCode, GetUtcNow()));
        });
    }

    private static string RequireSessionToken(string? sessionToken)
    {
        return sessionToken ?? throw SessionTranslator.ToFault(new InvalidSessionException());
    }

    private TResult Run<TResult>(string operationName, Func<IServiceProvider, TResult> operation)
    {
        try
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            return operation(scope.ServiceProvider);
        }
        catch (InvalidSessionException ex)
        {
            throw SessionTranslator.ToFault(ex);
        }
        catch (FriendshipRejectedException ex)
        {
            throw FriendshipTranslator.ToFault(ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.Error($"The friend operation could not be saved. Operation={operationName}", ex);
            throw new FaultException(ServerFailureReason);
        }
        catch (DbException ex)
        {
            _logger.Error($"The database could not be reached. Operation={operationName}", ex);
            throw new FaultException(ServerFailureReason);
        }
    }

    private DateTime GetUtcNow()
    {
        return _timeProvider.GetUtcNow().UtcDateTime;
    }
}
