using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bastion.Client.Tests.Fakes;

public sealed class ManualTimeProvider : TimeProvider
{
    private readonly List<ManualTimer> _timers;
    private DateTimeOffset _utcNow;

    public ManualTimeProvider(DateTimeOffset utcNow)
    {
        _timers = [];
        _utcNow = utcNow;
    }

    public override DateTimeOffset GetUtcNow()
    {
        return _utcNow;
    }

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        var timer = new ManualTimer(callback, state);
        _timers.Add(timer);
        return timer;
    }

    public void Advance(TimeSpan duration)
    {
        _utcNow += duration;
        List<ManualTimer> timersToFire = [.. _timers];
        foreach (ManualTimer timer in timersToFire)
        {
            timer.Fire();
        }
    }

    public int CountActiveTimers()
    {
        return _timers.Count(timer => !timer.IsDisposed);
    }

    private sealed class ManualTimer : ITimer
    {
        private readonly TimerCallback _callback;
        private readonly object? _state;

        public ManualTimer(TimerCallback callback, object? state)
        {
            _callback = callback;
            _state = state;
        }

        public bool IsDisposed { get; private set; }

        public bool Change(TimeSpan dueTime, TimeSpan period)
        {
            return !IsDisposed;
        }

        public void Fire()
        {
            if (IsDisposed)
            {
                return;
            }

            _callback(_state);
        }

        public void Dispose()
        {
            IsDisposed = true;
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
