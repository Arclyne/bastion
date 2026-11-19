using System;
using Bastion.Server.Application.Matches;
using Xunit;

namespace Bastion.Server.Application.Tests.Matches;

public sealed class TestMatchGate
{
    private const int MatchId = 31;
    private const int Result = 7;

    private readonly MatchGate _matchGate = new MatchGate();

    [Fact]
    public void Run_Function_ReturnsItsResult()
    {
        Func<int> operation = () => Result;

        int result = _matchGate.Run(MatchId, operation);

        Assert.Equal(Result, result);
    }

    [Fact]
    public void Run_Action_RunsIt()
    {
        bool hasRun = false;
        Action operation = () => hasRun = true;

        _matchGate.Run(MatchId, operation);

        Assert.True(hasRun);
    }

    [Fact]
    public void Run_NullFunction_ThrowsArgumentNullException()
    {
        Func<int>? operation = null;

        Func<object> run = () => _matchGate.Run(MatchId, operation!);

        Assert.Throws<ArgumentNullException>(run);
    }
}
