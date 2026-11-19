using System;

namespace Bastion.Server.Application.Matches;

public interface IMatchGate
{
    TResult Run<TResult>(int matchId, Func<TResult> operation);

    void Run(int matchId, Action operation);
}
