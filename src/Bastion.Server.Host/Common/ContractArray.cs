using System;
using System.Collections.Generic;

namespace Bastion.Server.Host.Common;

public static class ContractArray
{
    public static TItem[] ConvertAll<TSource, TItem>(IReadOnlyList<TSource> source, Func<TSource, TItem> convert)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(convert);

        var items = new TItem[source.Count];
        for (int index = 0; index < source.Count; index++)
        {
            items[index] = convert(source[index]);
        }

        return items;
    }
}
