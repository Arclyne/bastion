using System;
using System.Threading;

namespace Bastion.Client.Tests.Screens;

public static class InlineUiContext
{
    public static TViewModel CreateViewModel<TViewModel>(Func<TViewModel> createViewModel)
    {
        ArgumentNullException.ThrowIfNull(createViewModel);

        // View models post their notices to the context they are created on; without one they run inline.
        SynchronizationContext? previousContext = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(null);
        try
        {
            return createViewModel();
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(previousContext);
        }
    }
}
