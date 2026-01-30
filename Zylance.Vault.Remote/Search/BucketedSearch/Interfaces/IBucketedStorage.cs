#nullable disable

namespace Zylance.Vault.Remote.Search.BucketedSearch.Interfaces;

/// <summary>
///     Provides unified access to all storage components required by the bucketed search engine.
/// </summary>
/// <typeparam name="TItemId">The type of item identifier.</typeparam>
/// <remarks>
///     This interface aggregates the three core storage components of the bucketed search system:
///     glossary (keywords), buckets (item lists), and flags (index state tracking).
///     In zero-knowledge implementations, each component should handle encryption/decryption
///     of its respective data before persisting to remote storage.
/// </remarks>
public interface IBucketedStorage<TItemId>
    where TItemId : notnull
{
    /// <summary>
    ///     Gets the glossary for managing keyword metadata.
    /// </summary>
    /// <remarks>
    ///     The glossary tracks all indexed keywords and the number of buckets created for each.
    ///     In zero-knowledge implementations, keyword values should be encrypted or hashed.
    /// </remarks>
    IGlossary Glossary { get; }

    /// <summary>
    ///     Gets the bucket manager for storing and retrieving item ID lists.
    /// </summary>
    /// <remarks>
    ///     Buckets partition the inverted index to prevent unbounded growth of item lists.
    ///     When a bucket reaches maximum capacity, a new bucket is created for that keyword.
    ///     In zero-knowledge implementations, bucket contents should be encrypted.
    /// </remarks>
    IBucketManager<TItemId> Buckets { get; }

    /// <summary>
    ///     Gets the flag manager for tracking which items are currently indexed.
    /// </summary>
    /// <remarks>
    ///     Flags prevent duplicate indexing and enable efficient batch operations by
    ///     identifying which items need to be added, updated, or removed.
    ///     In zero-knowledge implementations, flags should be encrypted.
    /// </remarks>
    IFlagManager<TItemId> Flags { get; }
}
