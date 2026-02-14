using System.Text;

namespace Zylance.Vault.Local.Managers;

/// <summary>
///     Represents a cursor for ledger pagination using timestamp and ID.
///     Why use a composite cursor? Timestamps alone aren't unique (multiple entries could have
///     the same timestamp), so we include the ID to ensure stable, deterministic pagination.
/// </summary>
public record LedgerCursor
{
    /// <summary>
    ///     Default page size when not specified by the client.
    /// </summary>
    public const int DefaultPageSize = 50;

    /// <summary>
    ///     Maximum allowed page size to prevent excessive memory usage and long response times.
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// Cursor timestamp component (milliseconds since epoch).
    /// </summary>
    public long Timestamp { get; init; }

    /// <summary>
    /// Cursor id component to disambiguate identical timestamps.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    ///     Encodes the cursor to a base64 string for transport.
    ///     Format: timestamp|id
    /// </summary>
    public string Encode()
    {
        var raw = $"{Timestamp}|{Id}";
        var bytes = Encoding.UTF8.GetBytes(raw);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    ///     Decodes a cursor from a base64 string.
    /// </summary>
    public static LedgerCursor? Decode(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
            return null;

        try
        {
            var bytes = Convert.FromBase64String(cursor);
            var raw = Encoding.UTF8.GetString(bytes);
            var parts = raw.Split('|');

            return parts.Length != 2
                ? null
                : new LedgerCursor { Timestamp = long.Parse(parts[0]), Id = Guid.Parse(parts[1]) };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     Creates a cursor from a ledger entry.
    /// </summary>
    public static LedgerCursor FromEntry(long timestamp, Guid id)
    {
        return new LedgerCursor { Timestamp = timestamp, Id = id };
    }
}
