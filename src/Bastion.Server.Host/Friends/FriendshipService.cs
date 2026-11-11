using System;
using System.Collections.Generic;
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

/// <inheritdoc/>
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

    /// <inheritdoc/>
    public FriendList ListFriends(string? sessionToken)
    {
        string token = RequireSessionToken(sessionToken);
        Func<FriendList> listFriends = () => ListFriendsInRequestScope(token);
        return TranslateFriendshipFailures(nameof(ListFriends), listFriends);
    }

    /// <inheritdoc/>
    public PlayerSearchEntry[] SearchPlayers(string? sessionToken, string? text)
    {
        string token = RequireSessionToken(sessionToken);
        string searchText = text ?? string.Empty;
        Func<PlayerSearchEntry[]> searchPlayers = () => SearchPlayersInRequestScope(token, searchText);
        return TranslateFriendshipFailures(nameof(SearchPlayers), searchPlayers);
    }

    /// <inheritdoc/>
    public FriendRequestResult SendFriendRequest(string? sessionToken, string? friendCode)
    {
        string token = RequireSessionToken(sessionToken);
        string recipientCode = RequireFriendCode(friendCode, FriendshipRejection.PlayerNotFound);
        Func<FriendRequestResult> sendRequest = () => SendFriendRequestInRequestScope(token, recipientCode);
        return TranslateFriendshipFailures(nameof(SendFriendRequest), sendRequest);
    }

    /// <inheritdoc/>
    public FriendRequestList ListFriendRequests(string? sessionToken)
    {
        string token = RequireSessionToken(sessionToken);
        Func<FriendRequestList> listRequests = () => ListFriendRequestsInRequestScope(token);
        return TranslateFriendshipFailures(nameof(ListFriendRequests), listRequests);
    }

    /// <inheritdoc/>
    public void AcceptFriendRequest(string? sessionToken, string? requesterFriendCode)
    {
        string token = RequireSessionToken(sessionToken);
        string requesterCode = RequireFriendCode(requesterFriendCode, FriendshipRejection.RequestNotFound);
        Action acceptRequest = () => AcceptFriendRequestInRequestScope(token, requesterCode);
        TranslateFriendshipFailures(nameof(AcceptFriendRequest), acceptRequest);
    }

    /// <inheritdoc/>
    public void RejectFriendRequest(string? sessionToken, string? requesterFriendCode)
    {
        string token = RequireSessionToken(sessionToken);
        string requesterCode = RequireFriendCode(requesterFriendCode, FriendshipRejection.RequestNotFound);
        Action rejectRequest = () => RejectFriendRequestInRequestScope(token, requesterCode);
        TranslateFriendshipFailures(nameof(RejectFriendRequest), rejectRequest);
    }

    /// <inheritdoc/>
    public void CancelFriendRequest(string? sessionToken, string? recipientFriendCode)
    {
        string token = RequireSessionToken(sessionToken);
        string recipientCode = RequireFriendCode(recipientFriendCode, FriendshipRejection.RequestNotFound);
        Action cancelRequest = () => CancelFriendRequestInRequestScope(token, recipientCode);
        TranslateFriendshipFailures(nameof(CancelFriendRequest), cancelRequest);
    }

    /// <inheritdoc/>
    public void RemoveFriend(string? sessionToken, string? friendCode)
    {
        string token = RequireSessionToken(sessionToken);
        string removedFriendCode = RequireFriendCode(friendCode, FriendshipRejection.NotFriends);
        Action removeFriend = () => RemoveFriendInRequestScope(token, removedFriendCode);
        TranslateFriendshipFailures(nameof(RemoveFriend), removeFriend);
    }

    private static string RequireSessionToken(string? sessionToken)
    {
        return sessionToken ?? throw SessionTranslator.ToFault(new InvalidSessionException());
    }

    private static string RequireFriendCode(string? friendCode, FriendshipRejection rejectionWhenMissing)
    {
        return friendCode ?? throw FriendshipTranslator.ToFault(new FriendshipRejectedException(rejectionWhenMissing));
    }

    private FriendList ListFriendsInRequestScope(string token)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IFriendListService friendList = scope.ServiceProvider.GetRequiredService<IFriendListService>();
        FriendRoster roster = friendList.ListFriends(token, GetUtcNow());
        return FriendshipTranslator.ToFriendList(roster);
    }

    private PlayerSearchEntry[] SearchPlayersInRequestScope(string token, string searchText)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IFriendRequestService friendRequests = scope.ServiceProvider.GetRequiredService<IFriendRequestService>();
        IReadOnlyList<PlayerSearchResult> results = friendRequests.SearchPlayers(token, searchText, GetUtcNow());
        return FriendshipTranslator.ToSearchEntries(results);
    }

    private FriendRequestResult SendFriendRequestInRequestScope(string token, string recipientCode)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IFriendRequestService friendRequests = scope.ServiceProvider.GetRequiredService<IFriendRequestService>();
        FriendRequestOutcome outcome = friendRequests.SendFriendRequest(token, recipientCode, GetUtcNow());
        return FriendshipTranslator.ToFriendRequestResult(outcome);
    }

    private FriendRequestList ListFriendRequestsInRequestScope(string token)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IFriendRequestService friendRequests = scope.ServiceProvider.GetRequiredService<IFriendRequestService>();
        FriendRequestBoard board = friendRequests.ListFriendRequests(token, GetUtcNow());
        return FriendshipTranslator.ToFriendRequestList(board);
    }

    private void AcceptFriendRequestInRequestScope(string token, string requesterCode)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IFriendRequestService friendRequests = scope.ServiceProvider.GetRequiredService<IFriendRequestService>();
        friendRequests.AcceptFriendRequest(token, requesterCode, GetUtcNow());
    }

    private void RejectFriendRequestInRequestScope(string token, string requesterCode)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IFriendRequestService friendRequests = scope.ServiceProvider.GetRequiredService<IFriendRequestService>();
        friendRequests.RejectFriendRequest(token, requesterCode, GetUtcNow());
    }

    private void CancelFriendRequestInRequestScope(string token, string recipientCode)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IFriendRequestService friendRequests = scope.ServiceProvider.GetRequiredService<IFriendRequestService>();
        friendRequests.CancelFriendRequest(token, recipientCode, GetUtcNow());
    }

    private void RemoveFriendInRequestScope(string token, string removedFriendCode)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IFriendListService friendList = scope.ServiceProvider.GetRequiredService<IFriendListService>();
        friendList.RemoveFriend(token, removedFriendCode, GetUtcNow());
    }

    private TResult TranslateFriendshipFailures<TResult>(string operationName, Func<TResult> operation)
    {
        try
        {
            return operation();
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

    private void TranslateFriendshipFailures(string operationName, Action operation)
    {
        Func<bool> operationAsFunction = () => CompleteOperation(operation);
        TranslateFriendshipFailures(operationName, operationAsFunction);
    }

    private static bool CompleteOperation(Action operation)
    {
        operation();
        return true;
    }

    private DateTime GetUtcNow()
    {
        return _timeProvider.GetUtcNow().UtcDateTime;
    }
}
