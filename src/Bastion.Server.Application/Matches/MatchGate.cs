using System;
using System.Collections.Concurrent;

namespace Bastion.Server.Application.Matches;

public sealed class MatchGate : IMatchGate
{
    private readonly ConcurrentDictionary<int, object> _locksByMatchId;

    public MatchGate()
    {
        _locksByMatchId = new ConcurrentDictionary<int, object>();
    }

    public TResult Run<TResult>(int matchId, Func<TResult> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        object matchLock = _locksByMatchId.GetOrAdd(matchId, CreateLock);
        lock (matchLock)
        {
            return operation();
        }
    }

    public void Run(int matchId, Action operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        Func<bool> completeOperation = () => CompleteOperation(operation);
        Run(matchId, completeOperation);
    }

    private static object CreateLock(int matchId)
    {
        return new object();
    }

    private static bool CompleteOperation(Action operation)
    {
        operation();
        return true;
    }
}
