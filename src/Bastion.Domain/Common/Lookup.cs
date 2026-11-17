using System;

namespace Bastion.Domain.Common;

public static class Lookup
{
    public static Lookup<TValue> Found<TValue>(TValue value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Lookup<TValue>.CreateFound(value);
    }

    public static Lookup<TValue> Missing<TValue>(string searchedKey)
    {
        ArgumentNullException.ThrowIfNull(searchedKey);

        return Lookup<TValue>.CreateMissing(searchedKey);
    }

    public static Lookup<TValue> FromNullable<TValue>(TValue? value, string searchedKey)
        where TValue : class
    {
        ArgumentNullException.ThrowIfNull(searchedKey);

        if (value is null)
        {
            return Missing<TValue>(searchedKey);
        }

        return Found(value);
    }
}
