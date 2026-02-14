using Zylance.Core.Vault.Exceptions;

namespace Zylance.Core.Vault.Models;

/// <summary>
///     A concrete implementation of ICursorList for paginated results.
///     Why use records? Records provide value-based equality, immutability by
///     default,
///     and concise syntax - perfect for data structures like this that represent
///     immutable snapshots of paginated data.
/// </summary>
public record CursorList<T>
{
    /// <summary>
    ///     Cursor token for fetching the next page of results. Empty when there is no
    ///     next page.
    /// </summary>
    public required string Cursor { get; init; }

    /// <summary>
    ///     Total number of items across all pages.
    /// </summary>
    public required ulong TotalCount { get; init; }

    /// <summary>
    ///     Function that returns the next page of results. Callers should check
    ///     <see cref="HasNextPage" /> before invoking this to avoid
    ///     <see cref="CursorException" />.
    /// </summary>
    public Func<Task<CursorList<T>>>? NextPage { get; init; }

    /// <summary>
    ///     Indicates whether this page is the last page of results.
    /// </summary>
    public bool HasNextPage => NextPage is not null;

    /// <summary>
    ///     The items contained in this page.
    /// </summary>
    public required IReadOnlyList<T> Items { get; init; }
}
