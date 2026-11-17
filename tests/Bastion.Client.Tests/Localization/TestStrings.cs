using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text.RegularExpressions;
using Bastion.Client.Localization;
using Bastion.Domain.Text;
using Xunit;

namespace Bastion.Client.Tests.Localization;

public sealed class TestStrings
{
    private const string NeutralCultureName = "";
    private const string SpanishCultureName = "es-MX";

    private static readonly CultureInfo _spanishCulture = CultureInfo.GetCultureInfo(SpanishCultureName);
    private static readonly string[] _neutralOnlyKeys =
    {
        nameof(Strings.AppName),
        nameof(Strings.AppVersionFormat),
        nameof(Strings.LanguageNameEnglish),
        nameof(Strings.LanguageNameSpanish),
    };
    private static readonly string[] _brokenCharacterMarks = { "�", "Ã", "Â", "â€" };
    private static readonly Regex _placeholderPattern = TextPattern.Create(@"\{\d+(?:[,:][^}]*)?\}");

    [Fact]
    public void GetResourceSet_SpanishCulture_TranslatesEveryKey()
    {
        Dictionary<string, string> neutralTextsByKey = ReadNeutralTexts();
        IEnumerable<string> translatableKeys = neutralTextsByKey.Keys
            .Except(_neutralOnlyKeys)
            .Order(StringComparer.Ordinal);
        var expectedKeys = new List<string>(translatableKeys);

        ResourceSet? spanishSet = Strings.ResourceManager.GetResourceSet(
            _spanishCulture,
            createIfNotExists: true,
            tryParents: false);

        Dictionary<string, string> spanishTextsByKey = ReadTexts(spanishSet);
        IEnumerable<string> sortedSpanishKeys = spanishTextsByKey.Keys.Order(StringComparer.Ordinal);
        var spanishKeys = new List<string>(sortedSpanishKeys);
        Assert.Equal(expectedKeys, spanishKeys);
    }

    [Theory]
    [InlineData(NeutralCultureName)]
    [InlineData(SpanishCultureName)]
    public void GetResourceSet_AnyLanguage_HasNoBrokenCharacters(string cultureName)
    {
        var culture = CultureInfo.GetCultureInfo(cultureName);

        ResourceSet? resourceSet = Strings.ResourceManager.GetResourceSet(
            culture,
            createIfNotExists: true,
            tryParents: false);

        Dictionary<string, string> textsByKey = ReadTexts(resourceSet);
        IEnumerable<string> brokenKeys = textsByKey
            .Where(text => HasBrokenCharacters(text.Value))
            .Select(text => text.Key);
        Assert.Empty(brokenKeys);
    }

    [Fact]
    public void GetResourceSet_SpanishCulture_KeepsEveryPlaceholder()
    {
        Dictionary<string, string> neutralTextsByKey = ReadNeutralTexts();

        ResourceSet? spanishSet = Strings.ResourceManager.GetResourceSet(
            _spanishCulture,
            createIfNotExists: true,
            tryParents: false);

        Dictionary<string, string> spanishTextsByKey = ReadTexts(spanishSet);
        IEnumerable<string> keysWithOtherPlaceholders = spanishTextsByKey
            .Where(spanishText => HasOtherPlaceholders(spanishText, neutralTextsByKey))
            .Select(spanishText => spanishText.Key);
        Assert.Empty(keysWithOtherPlaceholders);
    }

    private static Dictionary<string, string> ReadNeutralTexts()
    {
        ResourceSet? neutralSet = Strings.ResourceManager.GetResourceSet(
            CultureInfo.InvariantCulture,
            createIfNotExists: true,
            tryParents: false);
        return ReadTexts(neutralSet);
    }

    private static Dictionary<string, string> ReadTexts(ResourceSet? resourceSet)
    {
        var textsByKey = new Dictionary<string, string>();
        if (resourceSet is null)
        {
            return textsByKey;
        }

        foreach (DictionaryEntry entry in resourceSet)
        {
            if (entry.Key is not string key || entry.Value is not string text)
            {
                continue;
            }

            textsByKey.Add(key, text);
        }

        return textsByKey;
    }

    private static bool HasBrokenCharacters(string text)
    {
        return _brokenCharacterMarks.Any(text.Contains);
    }

    private static bool HasOtherPlaceholders(
        KeyValuePair<string, string> spanishText,
        Dictionary<string, string> neutralTextsByKey)
    {
        string neutralText = neutralTextsByKey.GetValueOrDefault(spanishText.Key, string.Empty);
        List<string> neutralPlaceholders = ListPlaceholders(neutralText);
        List<string> spanishPlaceholders = ListPlaceholders(spanishText.Value);
        bool hasSamePlaceholders = neutralPlaceholders.SequenceEqual(spanishPlaceholders);
        return !hasSamePlaceholders;
    }

    private static List<string> ListPlaceholders(string text)
    {
        IEnumerable<string> placeholders = _placeholderPattern.Matches(text)
            .Select(match => match.Value)
            .Order(StringComparer.Ordinal);
        return new List<string>(placeholders);
    }
}
