using System;
using System.Text.RegularExpressions;

namespace Bastion.Domain.Text;

public static class TextPattern
{
    private const int TimeoutMilliseconds = 250;

    private static readonly TimeSpan _timeout = TimeSpan.FromMilliseconds(TimeoutMilliseconds);

    public static Regex Create(string pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);

        var regex = new Regex(pattern, RegexOptions.CultureInvariant, _timeout);
        return regex;
    }
}
