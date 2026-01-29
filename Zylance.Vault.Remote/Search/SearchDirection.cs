namespace Zylance.Vault.Remote.Search;

/// <summary>
/// Specifies the direction to traverse keyword buckets during search operations.
/// </summary>
/// <remarks>
/// The direction affects the order in which items appear in search results, with more recently
/// indexed items appearing in higher-numbered buckets. This allows prioritizing recent or
/// historical results without full result set sorting.
/// </remarks>
public enum SearchDirection
{
    /// <summary>
    /// Traverse buckets from the highest index to lowest, returning most recently indexed items first.
    /// </summary>
    /// <remarks>
    /// For a keyword with 3 buckets, this processes buckets in order: 2, 1, 0.
    /// Useful for showing recent transactions or activity first.
    /// </remarks>
    LatestFirst,

    /// <summary>
    /// Traverse buckets from the lowest index to highest, returning oldest indexed items first.
    /// </summary>
    /// <remarks>
    /// For a keyword with 3 buckets, this processes buckets in order: 0, 1, 2.
    /// Useful for chronological ordering or historical analysis.
    /// </remarks>
    OldestFirst,
}
