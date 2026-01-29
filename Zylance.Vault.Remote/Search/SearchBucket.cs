namespace Zylance.Vault.Remote.Search;

public readonly record struct BucketId(string Value);

/// <summary>
/// Represents a bucket containing item IDs for a specific keyword.
/// </summary>
/// <typeparam name="TItemId">
/// The type of item identifier. In zero-knowledge scenarios, should be encrypted identifiers.
/// </typeparam>
/// <remarks>
/// <para>
/// Buckets partition the search index to prevent unbounded growth of item lists. When a bucket
/// reaches the configured maximum size, a new bucket is created for the same keyword.
/// </para>
/// <para>
/// In zero-knowledge architecture, the entire bucket (ID and item list) should be encrypted
/// before storage. The server stores encrypted blobs without knowledge of the keyword or items.
/// </para>
/// </remarks>
public record SearchBucket<TItemId>
{
    /// <summary>
    /// Gets the bucket identifier, typically formatted as "{keyword}:{index}".
    /// </summary>
    /// <remarks>
    /// For example: "coffee:0", "coffee:1", "payment:0". The keyword portion may be encrypted
    /// or hashed in zero-knowledge implementations.
    /// </remarks>
    public required BucketId Id { get; init; }

    /// <summary>
    /// Gets the list of item IDs contained in this bucket.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The list grows as items are indexed until it reaches the maximum bucket size, at which
    /// point a new bucket is created for the keyword.
    /// </para>
    /// <para>
    /// <b>Note:</b> The current implementation allows duplicate item IDs. If the same item is
    /// indexed multiple times for the same keyword, it will appear multiple times in this list.
    /// </para>
    /// </remarks>
    public List<TItemId> ItemIds { get; init; } = [];
}
