using System;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Bastion.Server.Adapters.Persistence.Conversions;

public sealed class UpperSnakeCaseEnumConverter<TEnum> : ValueConverter<TEnum, string>
    where TEnum : struct, Enum
{
    private const char Separator = '_';
    private const int MaximumCodeLengthFactor = 2;

    public UpperSnakeCaseEnumConverter()
        : base(value => ToCode(value), code => FromCode(code))
    {
    }

    private static string ToCode(TEnum value)
    {
        string name = value.ToString();
        var builder = new StringBuilder(name.Length * MaximumCodeLengthFactor);
        for (int letterIndex = 0; letterIndex < name.Length; letterIndex++)
        {
            char letter = name[letterIndex];
            if (letterIndex > 0 && char.IsUpper(letter))
            {
                builder.Append(Separator);
            }

            char upperLetter = char.ToUpperInvariant(letter);
            builder.Append(upperLetter);
        }

        return builder.ToString();
    }

    private static TEnum FromCode(string code)
    {
        string[] words = code.Split(Separator);
        string name = string.Concat(words);
        return Enum.Parse<TEnum>(name, ignoreCase: true);
    }
}
