using System;
using System.Collections.Generic;

namespace Bastion.Server.Host.Common;

public static class ContractArray
{
    public static TItem[] ConvertAll<TSource, TItem>(IReadOnlyList<TSource> sourceItems, Func<TSource, TItem> converter)
    {
        ArgumentNullException.ThrowIfNull(sourceItems);
        ArgumentNullException.ThrowIfNull(converter);

        var items = new TItem[sourceItems.Count];
        for (int index = 0; index < sourceItems.Count; index++)
        {
            items[index] = converter(sourceItems[index]);
        }

        return items;
    }
}
