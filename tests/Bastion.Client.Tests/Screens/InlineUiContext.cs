using System;
using System.Threading;

namespace Bastion.Client.Tests.Screens;

public static class InlineUiContext
{
    public static TViewModel CreateViewModel<TViewModel>(Func<TViewModel> viewModelFactory)
    {
        SynchronizationContext? previousContext = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(null);
        try
        {
            return viewModelFactory();
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(previousContext);
        }
    }
}
