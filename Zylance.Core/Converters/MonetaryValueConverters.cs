using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Zylance.Core.Models;

namespace Zylance.Core.Converters;

/// <summary>
///     Entity Framework Core value converters for MonetaryValue.
///     Use these in your DbContext's OnModelCreating method to configure how MonetaryValue is stored.
/// </summary>
public static class MonetaryValueConverters
{
    /// <summary>
    ///     Value converter for non-nullable MonetaryValue properties.
    ///     Converts between MonetaryValue and int for database storage.
    /// </summary>
    public static ValueConverter<MonetaryValue, int> MonetaryValueConverter { get; } =
        new(
            v => v.Cents,
            v => new MonetaryValue(v)
        );

    /// <summary>
    ///     Value converter for nullable MonetaryValue properties.
    ///     Converts between MonetaryValue? and int? for database storage.
    /// </summary>
    public static ValueConverter<MonetaryValue?, int?> NullableMonetaryValueConverter { get; } =
        new(
            v => v.HasValue
                ? v.Value.Cents
                : null,
            v => v.HasValue
                ? new MonetaryValue(v.Value)
                : null
        );
}
