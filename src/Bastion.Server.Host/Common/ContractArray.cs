using System;
using System.Collections.Generic;

namespace Bastion.Server.Host.Common;

public static class ContractArray
{
    public static TItem[] ConvertAll<TSource, TItem>(IReadOnlyList<TSource> sourceItems, Func<TSource, TItem> convert)
    {
        ArgumentNullException.ThrowIfNull(sourceItems);
        ArgumentNullException.ThrowIfNull(convert);

        var items = new TItem[sourceItems.Count];
        for (int index = 0; index < sourceItems.Count; index++)
        {
            items[index] = convert(sourceItems[index]);
        }

        return items;
    }
}
