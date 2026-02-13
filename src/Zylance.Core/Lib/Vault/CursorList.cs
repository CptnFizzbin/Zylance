namespace Zylance.Core.Lib.Vault;

/// <summary>
///     A concrete implementation of ICursorList for paginated results.
///     Why use records? Records provide value-based equality, immutability by default,
///     and concise syntax - perfect for data structures like this that represent
///     immutable snapshots of paginated data.
/// </summary>
public record CursorList<T>
{
    /// <summary>
    /// Cursor token for fetching the next page of results. Empty when there is no next page.
    /// </summary>
    public required string NextCursor { get; init; }

    /// <summary>
    /// Total number of items across all pages.
    /// </summary>
    public required ulong TotalCount { get; init; }

    /// <summary>
    /// Indicates whether this page is the last page of results.
    /// </summary>
    public required bool IsLastPage { get; init; }

    /// <summary>
    /// The items contained in this page.
    /// </summary>
    public required IReadOnlyList<T> Items { get; init; }

    /// <summary>
    ///     Creates a simple cursor list from a complete list of items (no pagination).
    /// </summary>
    public static CursorList<T> FromList(IReadOnlyList<T> items)
    {
        return new CursorList<T>
        {
            NextCursor = string.Empty,
            TotalCount = (ulong)items.Count,
            IsLastPage = true,
            Items = items,
        };
    }

    /// <summary>
    ///     Creates a paginated cursor list.
    /// </summary>
    public static CursorList<T> Create(IReadOnlyList<T> items, string nextCursor, ulong totalCount, bool isLastPage)
    {
        return new CursorList<T>
        {
            Items = items,
            NextCursor = nextCursor,
            TotalCount = totalCount,
            IsLastPage = isLastPage,
        };
    }
}
