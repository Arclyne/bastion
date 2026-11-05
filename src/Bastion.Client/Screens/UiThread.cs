using System;
using System.Threading;

namespace Bastion.Client.Screens;

public static class UiThread
{
    public static void Run(SynchronizationContext? uiContext, Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (uiContext is null)
        {
            action();
            return;
        }

        uiContext.Post(_ => action(), null);
    }
}
