using System;
using System.Threading;

namespace Bastion.Client.Tests.Screens;

public static class InlineUiContext
{
    public static TViewModel CreateViewModel<TViewModel>(Func<TViewModel> createViewModel)
    {
        ArgumentNullException.ThrowIfNull(createViewModel);

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
