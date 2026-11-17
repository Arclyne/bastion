using System;

namespace Bastion.Domain.Common;

public abstract class Lookup<TValue>
{
    private Lookup(bool isFound)
    {
        IsFound = isFound;
    }

    public bool IsFound { get; }

    public abstract TValue GetValue();

    internal static Lookup<TValue> CreateFound(TValue value)
    {
        return new FoundLookup(value);
    }

    internal static Lookup<TValue> CreateMissing(string searchedKey)
    {
        return new MissingLookup(searchedKey);
    }

    private sealed class FoundLookup : Lookup<TValue>
    {
        private readonly TValue _value;

        public FoundLookup(TValue value)
            : base(isFound: true)
        {
            _value = value;
        }

        public override TValue GetValue()
        {
            return _value;
        }
    }

    private sealed class MissingLookup : Lookup<TValue>
    {
        private readonly string _searchedKey;

        public MissingLookup(string searchedKey)
            : base(isFound: false)
        {
            _searchedKey = searchedKey;
        }

        public override TValue GetValue()
        {
            throw new InvalidOperationException($"No {typeof(TValue).Name} was found. {_searchedKey}");
        }
    }
}
