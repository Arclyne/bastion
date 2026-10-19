using System;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Bastion.Server.Adapters.Persistence.Conversions;

public sealed class UpperSnakeCaseEnumConverter<TEnum> : ValueConverter<TEnum, string>
    where TEnum : struct, Enum
{
    private const char Separator = '_';

    public UpperSnakeCaseEnumConverter()
        : base(value => ToCode(value), code => FromCode(code))
    {
    }

    public static string ToCode(TEnum value)
    {
        string name = value.ToString();
        var builder = new StringBuilder(name.Length * 2);
        for (int letterIndex = 0; letterIndex < name.Length; letterIndex++)
        {
            if (letterIndex > 0 && char.IsUpper(name[letterIndex]))
            {
                builder.Append(Separator);
            }

            builder.Append(char.ToUpperInvariant(name[letterIndex]));
        }

        return builder.ToString();
    }

    public static TEnum FromCode(string code)
    {
        string name = code.Replace(Separator.ToString(), string.Empty, StringComparison.Ordinal);
        return Enum.Parse<TEnum>(name, ignoreCase: true);
    }
}
