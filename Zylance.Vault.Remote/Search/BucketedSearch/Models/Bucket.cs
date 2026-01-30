namespace Zylance.Vault.Remote.Search.BucketedSearch.Models;

/// <summary>
///     Represents a unique identifier for a bucket.
/// </summary>
/// <param name="Value">
///     The bucket identifier string, typically formatted as "{keyword}:{index}" (e.g., "coffee:0").
/// </param>
/// <remarks>
///     In zero-knowledge implementations, this value should be a hashed or encrypted representation
///     to prevent the storage layer from knowing which keyword the bucket belongs to.
/// </remarks>
public readonly record struct BucketId(string Value);

/// <summary>
///     Represents a bucket containing item IDs for a specific keyword.
/// </summary>
/// <typeparam name="TItemId">
///     The type of item identifier. In zero-knowledge scenarios, should be encrypted identifiers.
/// </typeparam>
/// <remarks>
///     <para>
///         Buckets partition the search index to prevent unbounded growth of item lists. When a bucket
///         reaches the configured maximum size, a new bucket is created for the same keyword.
///     </para>
///     <para>
///         In zero-knowledge architecture, the entire bucket (ID and item list) should be encrypted
///         before storage. The server stores encrypted blobs without knowledge of the keyword or items.
///     </para>
/// </remarks>
public record Bucket<TItemId>
{
    /// <summary>
    ///     Gets the bucket identifier, typically formatted as "{keyword}:{index}".
    /// </summary>
    /// <remarks>
    ///     For example: "coffee:0", "coffee:1", "payment:0". The keyword portion may be encrypted
    ///     or hashed in zero-knowledge implementations.
    /// </remarks>
    public required BucketId Id { get; init; }

    /// <summary>
    ///     Gets the list of item IDs contained in this bucket.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The list grows as items are indexed until it reaches the maximum bucket size, at which
    ///         point a new bucket is created for the keyword.
    ///     </para>
    /// </remarks>
    public HashSet<TItemId> ItemIds { get; init; } = [];
}
