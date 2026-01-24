namespace Zylance.Core.Models;

/// <summary>
///     Represents a monetary value stored in cents to avoid floating-point precision issues.
///     This is a value object that provides type safety and arithmetic operations for money.
/// </summary>
public readonly struct MonetaryValue : IEquatable<MonetaryValue>, IComparable<MonetaryValue>
{
    /// <summary>
    ///     The amount in cents (e.g., $10.50 = 1050).
    /// </summary>
    public int Cents { get; }

    /// <summary>
    ///     Creates a new MonetaryValue from cents.
    /// </summary>
    /// <param name="cents">Amount in cents</param>
    public MonetaryValue(int cents)
    {
        Cents = cents;
    }

    /// <summary>
    ///     Creates a MonetaryValue from a dollar amount.
    /// </summary>
    /// <param name="dollars">Amount in dollars (will be converted to cents)</param>
    /// <returns>A new MonetaryValue</returns>
    public static MonetaryValue FromDollars(decimal dollars)
    {
        return new MonetaryValue((int)(dollars * 100));
    }

    /// <summary>
    ///     Gets the dollar value as a decimal (e.g., 1050 cents = 10.50 dollars).
    /// </summary>
    public decimal Dollars => Cents / 100m;

    /// <summary>
    ///     Formats the monetary value as a currency string (e.g., "$10.50").
    /// </summary>
    public string ToFormattedString()
    {
        return $"${Dollars:F2}";
    }

    // Arithmetic operators
    public static MonetaryValue operator +(MonetaryValue left, MonetaryValue right)
    {
        return new MonetaryValue(left.Cents + right.Cents);
    }

    public static MonetaryValue operator -(MonetaryValue left, MonetaryValue right)
    {
        return new MonetaryValue(left.Cents - right.Cents);
    }

    public static MonetaryValue operator *(MonetaryValue value, decimal multiplier)
    {
        return new MonetaryValue((int)(value.Cents * multiplier));
    }

    public static MonetaryValue operator /(MonetaryValue value, decimal divisor)
    {
        return new MonetaryValue((int)Math.Round(value.Cents / divisor));
    }

    // Comparison operators
    public static bool operator ==(MonetaryValue left, MonetaryValue right)
    {
        return left.Cents == right.Cents;
    }

    public static bool operator !=(MonetaryValue left, MonetaryValue right)
    {
        return left.Cents != right.Cents;
    }

    public static bool operator <(MonetaryValue left, MonetaryValue right)
    {
        return left.Cents < right.Cents;
    }

    public static bool operator >(MonetaryValue left, MonetaryValue right)
    {
        return left.Cents > right.Cents;
    }

    public static bool operator <=(MonetaryValue left, MonetaryValue right)
    {
        return left.Cents <= right.Cents;
    }

    public static bool operator >=(MonetaryValue left, MonetaryValue right)
    {
        return left.Cents >= right.Cents;
    }

    public bool Equals(MonetaryValue other)
    {
        return Cents == other.Cents;
    }

    public override bool Equals(object? obj)
    {
        return obj is MonetaryValue other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Cents.GetHashCode();
    }

    public int CompareTo(MonetaryValue other)
    {
        return Cents.CompareTo(other.Cents);
    }

    public override string ToString()
    {
        return ToFormattedString();
    }

    public static MonetaryValue Zero => new(0);
}
