using System;
using Bastion.Server.Adapters.Persistence.Conversions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bastion.Server.Adapters.Persistence.Configurations;

public static class EnumColumnMapping
{
    public static PropertyBuilder<TEnum> HasUpperSnakeCaseConversion<TEnum>(
        this PropertyBuilder<TEnum> property,
        int maximumLength)
        where TEnum : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(property);

        StoreAsCode<TEnum>(property, maximumLength);
        return property;
    }

    public static PropertyBuilder<TEnum?> HasUpperSnakeCaseConversion<TEnum>(
        this PropertyBuilder<TEnum?> property,
        int maximumLength)
        where TEnum : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(property);

        StoreAsCode<TEnum>(property, maximumLength);
        return property;
    }

    private static void StoreAsCode<TEnum>(PropertyBuilder property, int maximumLength)
        where TEnum : struct, Enum
    {
        property.HasConversion(new UpperSnakeCaseEnumConverter<TEnum>());
        property.HasMaxLength(maximumLength);
        property.IsUnicode(false);
    }
}
