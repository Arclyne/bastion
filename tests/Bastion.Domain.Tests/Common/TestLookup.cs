using System;
using Bastion.Domain.Common;
using Xunit;

namespace Bastion.Domain.Tests.Common;

public sealed class TestLookup
{
    private const string FoundText = "found";
    private const string SearchedKey = "MatchId=42";

    [Fact]
    public void GetValue_FoundValue_ReturnsValue()
    {
        var lookup = Lookup.Found(FoundText);

        string value = lookup.GetValue();

        Assert.Equal(FoundText, value);
    }

    [Fact]
    public void GetValue_MissingValue_ThrowsInvalidOperationException()
    {
        var lookup = Lookup.Missing<string>(SearchedKey);
        Func<string> getValue = lookup.GetValue;

        Assert.Throws<InvalidOperationException>(getValue);
    }

    [Fact]
    public void GetValue_MissingValue_NamesSearchedKey()
    {
        var lookup = Lookup.Missing<string>(SearchedKey);
        Func<string> getValue = lookup.GetValue;

        Exception? exception = Record.Exception(getValue);

        Assert.EndsWith(SearchedKey, exception?.Message);
    }

    [Fact]
    public void FromNullable_NullValue_IsMissing()
    {
        var lookup = Lookup.FromNullable<string>(value: null, SearchedKey);

        Assert.False(lookup.IsFound);
    }

    [Fact]
    public void FromNullable_Value_IsFound()
    {
        var lookup = Lookup.FromNullable(FoundText, SearchedKey);

        Assert.True(lookup.IsFound);
    }
}
